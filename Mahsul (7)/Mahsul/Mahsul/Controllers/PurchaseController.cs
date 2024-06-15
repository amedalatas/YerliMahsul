using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;

namespace Mahsul.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly SmtpSettings _smtpSettings;

        public PurchaseController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, SmtpSettings smtpSettings)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _smtpSettings = smtpSettings;
        }

        [Authorize(Roles = "Admin, Farmer")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Farmer"))
            {
                var productIds = await _context.Product
                    .Where(p => p.Farmer.UserID == currentUser.Id)
                    .Select(p => p.ProductID)
                    .ToListAsync();

                var purchases = await _context.Purchase
                    .Include(p => p.User)
                    .Include(p => p.Product)
                    .Where(p => productIds.Contains(p.ProductID))
                    .ToListAsync();

                return View(purchases);
            }
            else // Admin ise, tüm satın alımları görebilsin
            {
                var purchases = await _context.Purchase
                    .Include(p => p.User)
                    .Include(p => p.Product)
                    .ToListAsync();
                return View(purchases);
            }
        }
        public async Task<IActionResult> Purchases()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var purchases = await _context.Purchase
                .Include(p => p.Product)
                .Where(p => p.UserID == currentUser.Id)
                .ToListAsync();

            return View(purchases);
        }


        // GET: PurchaseController/PurchaseProduct
        [HttpGet]
        public async Task<IActionResult> PurchaseProduct(int ProductID)
        {
            var product = await _context.Product
                .Include(p => p.ProductImage) // Include ProductImage
                .FirstOrDefaultAsync(p => p.ProductID == ProductID);

            if (product == null)
            {
                return RedirectToAction("FullIndex", "Product");
            }
            var farmer = await _context.farmers
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.FarmerID == product.FarmerID);
            var model = new PurchaseViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Description = product.Description,
                Stok = product.Stok,
                SelectedCategoryName = product.SelectedCategoryName,
                Address = product.Address,
                FarmerID = product.FarmerID,
                FarmerUsername = farmer?.User?.UserName, // Retrieve the username

                ProductImages = product.ProductImage.ToList() // Add ProductImage list to the model
            };

            return View(model);
        }






        [HttpPost]
        public async Task<IActionResult> PurchaseProduct([FromForm] PurchaseViewModel model)
        {
            var product = await _context.Product.FindAsync(model.ProductID);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Satın almak istediğiniz ürün bulunamadı.";
                return RedirectToAction("FullIndex", "Product");
            }

            if (model.Quantity <= 0 || model.Quantity > product.Stok)
            {
                TempData["ErrorMessage"] = "Geçersiz miktar veya stoğun yetersiz olması.";
                return View(model);
            }

            // Kart bilgilerini kontrol edin (Bu senaryo için her zaman true dönecek)
            if (string.IsNullOrEmpty(model.CardNumber) || string.IsNullOrEmpty(model.CardHolderName) ||
                string.IsNullOrEmpty(model.ExpiryDate) || string.IsNullOrEmpty(model.CVC))
            {
                TempData["ErrorMessage"] = "Kart bilgileri eksik.";
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var purchase = new Purchase
            {
                PurchaseDate = DateTime.Now,
                UserID = userId,
                ProductID = model.ProductID,
                Quantity = model.Quantity,
                Price = product.Price,
            };

            product.Stok -= model.Quantity;

            _context.Purchase.Add(purchase);
            await _context.SaveChangesAsync();
            await NotifyFarmer(product.FarmerID, product.ProductName, model.Quantity);

            return RedirectToAction("FullIndex", "Product");
        }

       

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.am")
            {
                Port = 587,
                Credentials = new NetworkCredential("your-email@example.com", "your-email-password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@example.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPurchase([FromForm] PurchaseViewModel model)
        {
            var product = await _context.Product.FindAsync(model.ProductID);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Satın almak istediğiniz ürün bulunamadı.";
                return RedirectToAction("FullIndex", "Product");
            }

            if (model.Quantity <= 0 || model.Quantity > product.Stok)
            {
                TempData["ErrorMessage"] = "Geçersiz miktar veya stoğun yetersiz olması.";
                return View("PurchaseProduct", model);
            }
            
            // Kart bilgilerini girmek için yönlendirme
            return View("EnterPaymentDetails", model);
        }
        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromForm] PurchaseViewModel model)
        {
            var product = await _context.Product.FindAsync(model.ProductID);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Satın almak istediğiniz ürün bulunamadı.";
                return RedirectToAction("FullIndex", "Product");
            }

            if (string.IsNullOrEmpty(model.CardNumber) || model.CardNumber.Length != 16 ||
                string.IsNullOrEmpty(model.CardHolderName) || !System.Text.RegularExpressions.Regex.IsMatch(model.CardHolderName, @"^[a-zA-Z\s]+$") ||
                string.IsNullOrEmpty(model.ExpiryMonth) || !System.Text.RegularExpressions.Regex.IsMatch(model.ExpiryMonth, @"^(0[1-9]|1[0-2])$") ||
                string.IsNullOrEmpty(model.ExpiryYear) || !System.Text.RegularExpressions.Regex.IsMatch(model.ExpiryYear, @"^\d{4}$") ||
                string.IsNullOrEmpty(model.CVC) || model.CVC.Length != 3)
            {
                TempData["ErrorMessage"] = "Kart bilgileri hatalı.";
                return View("EnterPaymentDetails", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var purchase = new Purchase
            {
                PurchaseDate = DateTime.Now,
                UserID = userId,
                ProductID = model.ProductID,
                Quantity = model.Quantity,
                Price = product.Price,
            };

            product.Stok -= model.Quantity;

            _context.Purchase.Add(purchase);
            await _context.SaveChangesAsync();

            // Çiftçiye bildirim e-postası gönder
            await NotifyFarmer(product.FarmerID, product.ProductName, model.Quantity);

            return RedirectToAction("PaymentSuccess", new { quantity = model.Quantity, totalPrice = product.Price * model.Quantity });
        }

        [HttpGet]
        public IActionResult PaymentSuccess(int quantity, decimal totalPrice)
        {
            ViewBag.Quantity = quantity;
            ViewBag.TotalPrice = totalPrice;
            return View();
        }
        private async Task NotifyFarmer(int farmerId, string productName, int quantity)
        {
            var farmer = await _context.farmers.FindAsync(farmerId);
            if (farmer == null || string.IsNullOrEmpty(farmer.Username))
            {
                // Çiftçi bulunamazsa veya e-posta adresi yoksa, işlem iptal edilsin.
                return;
            }

            var smtpClient = new SmtpClient(_smtpSettings.Server)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username),
                Subject = "Ürününüz Satıldı",
                Body = $"Merhaba, {productName} adlı ürününüzden {quantity} adet satılmıştır. Lütfen ürünü hazırlayınız.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(farmer.Username);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapılabilir
                Console.WriteLine($"E-posta gönderimi başarısız: {ex.Message}");
            }
        }

    }
}

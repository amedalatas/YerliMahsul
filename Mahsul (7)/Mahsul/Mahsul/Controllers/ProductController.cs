using Mahsul.Data;
using Mahsul.Data.Migrations;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Security.Claims;
using DateTime = System.DateTime;

namespace Mahsul.Controllers
{
    public class ProductController : Controller
    {
        IWebHostEnvironment hostEnvironment;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductController(IWebHostEnvironment hc, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            hostEnvironment = hc;
            _context = context;
            _userManager = userManager;
        }

        // GET: FarmerProductController1

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmer = _context.farmers.FirstOrDefault(f => f.UserID == userId);

            if (farmer == null)
            {
                // Giriş yapan kullanıcıya ait çiftçi kaydı bulunamadı.
                return NotFound();
            }

            var products = _context.Product
                .Where(p => p.FarmerID == farmer.FarmerID)
                .Include(p => p.ProductImage) // Resimleri de dahil et
                .ToList();

            return View(products);
        }

        public async Task<IActionResult> FullIndex(string[] selectedCategories)
        {
            var products = await _context.Product
                .Include(p => p.ProductImage) // Resimleri de dahil et
                .ToListAsync();
            var categories = await _context.categories.ToListAsync();

            foreach (var product in products)
            {
                if (product.Stok == 0)
                {
                    product.isActive = false;
                }
            }

            await _context.SaveChangesAsync();
            var activeProducts = products.Where(p => p.isActive).ToList();

            if (selectedCategories != null && selectedCategories.Length > 0)
            {
                activeProducts = activeProducts.Where(p => selectedCategories.Contains(p.SelectedCategoryName)).ToList();
            }

            // Aktif kullanıcının UserId'sini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kullanıcının FarmerId'sini bul
            var userFarmerId = await _context.farmers
                .Where(f => f.UserID == userId)
                .Select(f => f.FarmerID)
                .FirstOrDefaultAsync();

            var model = new ProductCategoryViewModel
            {
                Products = activeProducts,
                Categories = categories,
                SelectedCategories = selectedCategories,
                UserFarmerId = userFarmerId ,
               // FarmerUsername=use
                
            };

            return View(model);
        }


        [HttpGet]
        public IActionResult AddRating(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var product = _context.Product.FirstOrDefault(p => p.ProductID == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Farmers tablosundaki UserId ile eşleşen FarmerId'yi kontrol edelim
            var farmer = _context.farmers.FirstOrDefault(f => f.FarmerID == product.FarmerID && f.UserID == userId);

            if (farmer != null)
            {
                TempData["ErrorMessage"] = "Kendi ürününüze yorum yapamazsınız.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var viewModel = new RatingViewModel
            {
                ProductID = productId
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddRating(RatingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var product = _context.Product.FirstOrDefault(p => p.ProductID == model.ProductID);

            if (product == null)
            {
                return NotFound();
            }

            // Farmers tablosundaki UserId ile eşleşen FarmerId'yi kontrol edelim
            var farmer = _context.farmers.FirstOrDefault(f => f.FarmerID == product.FarmerID && f.UserID == userId);

            if (farmer != null)
            {
                TempData["ErrorMessage"] = "Kendi ürününüze yorum yapamazsınız.";
                return RedirectToAction("Details", "Product", new { id = model.ProductID });
            }

            var purchase = _context.Purchase.FirstOrDefault(p => p.ProductID == model.ProductID && p.UserID == userId);
            if (purchase == null)
            {
                ModelState.AddModelError("", "Yorum yapabilmek için ürünü satın almış olmanız gerekir.");
                ViewData["CustomError"] = "Yorum yapabilmek için ürünü satın almış olmanız gerekir.";
                return View(model);
            }

            var rating = new Rating
            {
                Username = _userManager.GetUserName(User),
                RatingValue = model.RatingValue,
                Comment = model.Comment,
                CreatedAt = DateTime.Now,
                UserID = userId,
                ProductID = model.ProductID
            };

            _context.Rating.Add(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Product", new { id = model.ProductID });
        }


        public async Task<IActionResult> Details(int id)
        {
            var product = _context.Product
            .Include(p => p.Ratings)
            .Include(p => p.ProductImage) // ProductImage'i include et
            .FirstOrDefault(p => p.ProductID == id);


            if (product == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Çiftçinin UserId'sini al
            var farmerUserId = _context.farmers
                .Where(f => f.FarmerID == product.FarmerID)
                .Select(f => f.UserID)
                .FirstOrDefault();

            string farmerUsername = null;

            // Farmer UserId'si varsa, ilgili kullanıcının username'ini al
            if (!string.IsNullOrEmpty(farmerUserId))
            {
                var farmerUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == farmerUserId);

                if (farmerUser != null)
                {
                    farmerUsername = farmerUser.UserName;
                }
            }

            var model = new ProductDetailsViewModel
            {
                Product = product,
                IsOwnProduct = farmerUserId == userId,
                FarmerUserName = farmerUsername
            };

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> ActiveStatus(int ProductID)
        {
            var product = await _context.Product.FindAsync(ProductID);

            if (product == null)
            {
                return NotFound();
            }

            product.isActive = !product.isActive;
            await _context.SaveChangesAsync();

            return RedirectToAction("FullIndex", "Product"); // veya başka bir sayfaya yönlendirme yapabilirsiniz
        }

        // GET: ProductController1/AddProduct
        [Authorize(Roles = "Farmer")]
        [HttpGet]
        public IActionResult AddProduct()
        {
            var viewModel = new ProductViewModel
            {
                categories = _context.categories.ToList()
            };
            return View(viewModel);
        }

        // Post: ProductController/AddProduct
        [HttpPost]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.categories = _context.categories.ToList();
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var farmer = _context.farmers.FirstOrDefault(f => f.UserID == userId);

            if (farmer == null)
            {
                ModelState.AddModelError("", "Giriş yapan kullanıcıya ait bir çiftçi kaydı bulunamadı.");
                model.categories = _context.categories.ToList();
                return View(model);
            }

            var selectedCategory = _context.categories.FirstOrDefault(c => c.CategoryId == model.SelectedCategoryId);
            if (selectedCategory == null)
            {
                ModelState.AddModelError("", "Geçersiz kategori seçimi.");
                model.categories = _context.categories.ToList();
                return View(model);
            }
            if (model.StartDate >= model.EndDate)
            {
                ModelState.AddModelError("", "Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
                model.categories = _context.categories.ToList();
                return View(model);
            }
            if (model.StartDate < DateTime.Today || model.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Başlangıç tarihi veya Bitiş tarihi bugünden önce olamaz.");
                model.categories = _context.categories.ToList();
                return View(model);

            }

            var product = new Product
            {
                ProductName = model.ProductName,
                Price = model.Price,
                Description = model.Description,
                Stok = model.Stok,
                Address = model.Address,
                SelectedCategoryId = model.SelectedCategoryId,
                SelectedCategoryName = selectedCategory.Name,
                FarmerID = farmer.FarmerID,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                isActive = model.StartDate == DateTime.Today,
                ProductImage = new List<ProductImage>() // İlişkiyi kurmak için güncellendi
            };

            if (model.photos != null && model.photos.Any())
            {
                foreach (var photo in model.photos)
                {
                    var filename = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    var filepath = Path.Combine(hostEnvironment.WebRootPath, "images", filename);
                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }

                    var productImage = new ProductImage
                    {
                        ImagePath = filename,
                        ProductID = product.ProductID // Ürünle ilişkilendirme
                    };
                    product.ProductImage.Add(productImage);// Ürünle ilişkilendirildi
                }
            }

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }






        // GET: ProductController/Edit
        [FarmerAuthorization]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Product.Include(p => p.ProductImage).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Description = product.Description,
                Stok = product.Stok,
                Address = product.Address,
                SelectedCategoryId = product.SelectedCategoryId ?? 0,
                categories = _context.categories.ToList(),
                ImagePaths = product.ProductImage.Select(pi => pi.ImagePath).ToList(),
                StartDate = product.StartDate,
                EndDate = product.EndDate
            };

            ViewBag.Categories = new SelectList(_context.categories, "CategoryId", "Name", viewModel.SelectedCategoryId);

            return View(viewModel);
        }

        [FarmerAuthorization]
        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.categories = _context.categories.ToList();
                ViewBag.Categories = new SelectList(_context.categories, "CategoryId", "Name", model.SelectedCategoryId);
                return View(model);
            }

            var product = await _context.Product.Include(p => p.ProductImage).FirstOrDefaultAsync(p => p.ProductID == model.ProductID);
            if (product == null)
            {
                return NotFound();
            }

            product.ProductName = model.ProductName;
            product.Price = model.Price;
            product.Description = model.Description;
            product.Stok = model.Stok;
            product.Address = model.Address;
            product.SelectedCategoryId = model.SelectedCategoryId;
            product.SelectedCategoryName = _context.categories.FirstOrDefault(c => c.CategoryId == model.SelectedCategoryId)?.Name;  // Eklendi
            product.StartDate = model.StartDate;
            product.EndDate = model.EndDate;

            if (model.photos != null && model.photos.Any())
            {
                foreach (var photo in model.photos)
                {
                    var filename = Guid.NewGuid().ToString() + "_" + photo.FileName;
                    var filepath = Path.Combine(hostEnvironment.WebRootPath, "images", filename);
                    using (var fileStream = new FileStream(filepath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }

                    var productImage = new ProductImage
                    {
                        ImagePath = filename,
                        ProductID = product.ProductID // Ürünle ilişkilendirme
                    };
                    product.ProductImage.Add(productImage);
                }
            }
            if (model.StartDate >= model.EndDate)
            {
                ModelState.AddModelError("", "Başlangıç tarihi bitiş tarihinden önce olmalıdır.");
                model.categories = _context.categories.ToList();
                ViewBag.Categories = new SelectList(_context.categories, "CategoryId", "Name", model.SelectedCategoryId);
                return View(model);
            }
            if (model.StartDate < DateTime.Today || model.EndDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Başlangıç tarihi veya Bitiş tarihi bugünden önce olamaz.");
                model.categories = _context.categories.ToList();
                ViewBag.Categories = new SelectList(_context.categories, "CategoryId", "Name", model.SelectedCategoryId);
                return View(model);
            }
            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Product");
        }








        // POST: ProductController1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // İlgili ProductID'ye sahip satış kayıtlarını buluyoruz
            var hasSales = _context.Purchase.Any(p => p.ProductID == id);

            if (hasSales)
            {
                // Satış yapılmışsa, silme işlemi gerçekleştirilmez
                TempData["ErrorMessage"] = "Silmek istediğiniz ürünün aktif olarak satışları bulunduğu için silme işlemi gerçekleştirilemez.";
                return RedirectToAction(nameof(Index));
            }

            // Satış yapılmamışsa, ürünü silebiliriz
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}

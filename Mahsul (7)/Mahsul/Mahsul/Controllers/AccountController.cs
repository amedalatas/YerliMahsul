using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Mahsul.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context,UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(user); // Kullanıcının rollerini alın

            // Eğer kullanıcı rolleri veya adresi yoksa, soruları göster
            if (!userRoles.Any())
            {
                return View(); // Eğer adres veya roller yoksa, soruları gösteren View'a yönlendir
            }
            else
            {
                // Kullanıcı rolleri veya adresi varsa, başka bir içerik gösterilebilir
                return RedirectToAction("FullIndex", "Product"); // Örneğin, başka bir Controller ve Action'a yönlendirin
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckUserRole(string answer, string address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Kullanıcı bulunamazsa veya oturum açmamışsa, uygun bir hata işleme mekanizması kullanabilirsiniz
                return RedirectToAction("Error");
            }

            if (string.IsNullOrEmpty(answer) || string.IsNullOrEmpty(address))
            {
                return View(); // Eğer cevap veya adres bilgisi yoksa, soruları gösteren View'a yönlendir
            }

            if (answer == "evet")
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "Farmer");
                if (!roleResult.Succeeded)
                {
                    // Rol atanırken bir hata oluşursa, uygun bir hata işleme mekanizması kullanabilirsiniz
                    return RedirectToAction("Error");
                }

                // Farmer tablosuna kullanıcıyı ekle
                var farmer = new Farmer
                {
                    UserID = user.Id,
                    Username = user.UserName,
                    Address = address // Kullanıcıdan alınan adres bilgisi
                };
                _context.farmers.Add(farmer);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Farmer ekleme sırasında bir hata oluşursa, uygun bir hata işleme mekanizması kullanabilirsiniz
                    // Hata detaylarını loglayabilir ve kullanıcıya anlamlı bir mesaj gösterebilirsiniz
                    Console.WriteLine($"Error adding farmer: {ex.Message}");
                    return RedirectToAction("Error");
                }
            }
            else if (answer == "hayır")
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    // Rol atanırken bir hata oluşursa, uygun bir hata işleme mekanizması kullanabilirsiniz
                    return RedirectToAction("Error");
                }
            }

            // evet veya hayır cevabı olsa da, Index sayfasına geri yönlendirin
            return RedirectToAction("FullIndex", "Product");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BecomeFarmer(string address)
        {
            // Kullanıcı giriş yapmamışsa, giriş yapması için yönlendir
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcı giriş yapmışsa ve "Farmer" rolüne sahip değilse, rolü ata
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !await _userManager.IsInRoleAsync(user, "Farmer"))
            {
                // Adres bilgisi boş olmamalı
                if (string.IsNullOrWhiteSpace(address))
                {
                    TempData["ErrorMessage"] = "Adres bilgisini girmeniz gerekmektedir.";
                    return RedirectToAction("FullIndex", "Product");
                }

                var result = await _userManager.AddToRoleAsync(user, "Farmer");

                // Farmer tablosuna yeni bir çiftçi ekleyin
                var farmer = new Farmer
                {
                    UserID = user.Id,
                    Address = address, // Kullanıcının girdiği adres bilgisini kullan
                    Username = user.UserName,
                    // Diğer gerekli özellikleri burada belirtin
                };

                // Farmer nesnesini veritabanına ekleyin
                _context.farmers.Add(farmer);
                await _context.SaveChangesAsync();

                // Rol atama başarılıysa, kullanıcıya başarı mesajı göster ve ana sayfaya yönlendir
                TempData["SuccessMessage"] = "Çiftçi olarak başarıyla kaydedildiniz. Artık ürün ekleyebilirsiniz.";
                return RedirectToAction("FullIndex", "Product");
            }

            // Kullanıcı zaten "Farmer" rolüne sahipse veya rol atama işlemi başarısız olduysa, hata mesajı göster ve ana sayfaya yönlendir
            TempData["ErrorMessage"] = "Çiftçi olarak kaydedilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            return RedirectToAction("FullIndex", "Product");
        }

    }
}

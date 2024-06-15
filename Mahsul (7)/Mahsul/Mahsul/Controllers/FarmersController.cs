using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mahsul.Controllers
{
    public class FarmersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarmersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var farmers = _context.farmers.ToList();

            // Her bir çiftçinin kullanıcı adını almak için döngüyü kullanın
            foreach (var farmer in farmers)
            {
                // Çiftçinin UserId'sini kullanarak ilgili kullanıcıyı bulun
                var user = _context.Users.FirstOrDefault(u => u.Id == farmer.UserID);
                if (user != null)
                {
                    // Kullanıcı adını Farmer modeline ekleyin
                    farmer.Username = user.UserName;
                }
            }

            return View(farmers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var farmer = _context.farmers.FirstOrDefault(f => f.FarmerID == id);
            if (farmer != null)
            {
                var products = _context.Product.Where(p => p.FarmerID == id).ToList();
                if (products.Any())
                {
                    // Eğer çiftçinin ürünleri varsa, kullanıcıya bir uyarı gösterin
                    TempData["ErrorMessage"] = "Bu çiftçinin aktif ürünleri bulunmaktadır. Silme işlemi gerçekleşemez.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Eğer çiftçinin ürünleri yoksa, silme işlemine devam edin
                    _context.farmers.Remove(farmer);

                    // UserRoles tablosundan ilgili çiftçinin rolünü kaldır
                    var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == farmer.UserID && ur.RoleId=="2");
                    if (userRole != null)
                    {
                        _context.UserRoles.Remove(userRole);
                    }

                    _context.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult RemoveProducts(int id)
        {
            var farmer = _context.farmers.FirstOrDefault(f => f.FarmerID == id);
            if (farmer != null)
            {
                // Çiftçinin ürün resimlerini sil
                var productImages = _context.ProductImage.Where(pi => pi.Product.FarmerID == id).ToList();
                _context.ProductImage.RemoveRange(productImages);
                _context.SaveChanges();

                // Çiftçinin ürünlerini sistemden kaldır
                var products = _context.Product.Where(p => p.FarmerID == id).ToList();
                _context.Product.RemoveRange(products);
                _context.SaveChanges();

            }
            return RedirectToAction(nameof(Index));
        }






        public IActionResult Details(int id)
        {
            var farmers = _context.farmers.FirstOrDefault(f => f.FarmerID == id);
            return View(farmers);
        }

        public IActionResult Edit(int id)
        {
            var farmers = _context.farmers.FirstOrDefault(f => f.FarmerID==id);
            return View(farmers);
        }
    }

}

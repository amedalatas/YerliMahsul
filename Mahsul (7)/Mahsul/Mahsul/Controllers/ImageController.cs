using Mahsul.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Mahsul.Controllers
{
    public class ImageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImageController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeleteImage(int productId, string imagePath)
        {
            var product = _context.Product.Include(p => p.ProductImage).FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return NotFound();
            }

            var image = product.ProductImage.FirstOrDefault(img => img.ImagePath == imagePath);
            if (image != null)
            {
                product.ProductImage.Remove(image);
                _context.SaveChanges();

                // Sunucudan dosyayı silmek için
                var filePath = Path.Combine(_environment.WebRootPath, "images", imagePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            return Ok();
        }
    }
}

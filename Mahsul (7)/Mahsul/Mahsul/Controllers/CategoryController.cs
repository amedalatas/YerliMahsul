using System;
using Microsoft.AspNetCore.Mvc;
using Mahsul.Models;
using System.Linq;
using Mahsul.Data;
using Microsoft.AspNetCore.Identity;

namespace Mahsul.Controllers
{
    public class CategoryController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: CategoryController1/Index
        public IActionResult Index()
        {
            var categories = _context.categories.ToList();

            return View(categories);
        }

        // GET: CategoryController1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoryController1/Create
        [HttpPost]
        public IActionResult Create(Category category)
        {
            _context.categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: CategoryController1/Edit/5
        public IActionResult Edit(int id)
        {
            var category = _context.categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: CategoryController1/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            var existingCategory = _context.categories.Find(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;

            _context.categories.Update(existingCategory);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: CategoryController1/Delete/5
        public IActionResult Delete(int id)
        {
            var category = _context.categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: CategoryController1/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            // Kategoride ürün var mı kontrolü
            var productsInCategory = _context.Product.Where(p => p.SelectedCategoryId == id).ToList();
            if (productsInCategory.Any())
            {
                // Kategoriye ait ürünler varsa, silme işlemini engelle ve uyarı göster
                TempData["ErrorMessage"] = "Bu kategoriye ait ürünler olduğu için kategori silinemez.";
                return RedirectToAction(nameof(Index));
            }

            // Kategoriye ait ürünler yoksa, silme işlemine devam et
            _context.categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

    }
}

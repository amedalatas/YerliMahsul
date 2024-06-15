using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mahsul.Controllers
{
    public class RatingController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        public RatingController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Edit(int id)
        {
            var rating = await _context.Rating.FindAsync(id);
            if (rating == null || rating.UserID != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var viewModel = new RatingViewModel
            {
                RatingID = rating.RatingID,
                ProductID = rating.ProductID,
                RatingValue = rating.RatingValue,
                Comment = rating.Comment
            };

            return View(viewModel);
        }

        // POST: Rating/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RatingViewModel model)
        {
            if (id != model.RatingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var rating = await _context.Rating.FindAsync(id);
                if (rating == null || rating.UserID != _userManager.GetUserId(User))
                {
                    return NotFound();
                }

                rating.RatingValue = model.RatingValue;
                rating.Comment = model.Comment;

                _context.Update(rating);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Product", new { id = rating.ProductID });
            }
            return View(model);
        }

        // POST: Rating/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _context.Rating.FindAsync(id);
            if (rating == null || rating.UserID != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            _context.Rating.Remove(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Product", new { id = rating.ProductID });
        }
    }
}


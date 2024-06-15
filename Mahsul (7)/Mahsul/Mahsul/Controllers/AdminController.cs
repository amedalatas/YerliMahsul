using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ApplicationDbContext _context;
    public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public IActionResult Statistics()
    {

        var totalUsers = _context.Users.Count();
        ViewData["TotalUsers"] = totalUsers;

        var totalConfirmedUsers = _context.Users.Where(u => u.EmailConfirmed).Count();
        ViewData["TotalConfirmedUsers"] = totalConfirmedUsers;

        var totalUsersUsers = _context.UserRoles
        .Where(ur => ur.RoleId == "3") // User rolüne sahip olan kayıtları seç
        .Select(ur => ur.UserId) // Kullanıcıların Id'lerini seç
        .Distinct() // Tekrar edenleri kaldır
        .Count(); // Toplam kullanıcı sayısını al
        ViewData["totalUsersUsers"] = totalUsersUsers;

        var totalAdmin = _context.UserRoles
        .Where(ur => ur.RoleId == "1") // User rolüne sahip olan kayıtları seç
        .Select(ur => ur.UserId) // Kullanıcıların Id'lerini seç
        .Distinct() // Tekrar edenleri kaldır
        .Count(); // Toplam kullanıcı sayısını al
        ViewData["totalAdmin"] = totalAdmin;

        var totalFarmers = _context.farmers.Count();
        ViewData["TotalFarmers"] = totalFarmers;
       

        var totalPurchases = _context.Purchase.Count();
        ViewData["TotalPurchases"] = totalPurchases;

        var totalProduct = _context.Product.Count();
        ViewData["TotalProduct"] = totalProduct;

        var totalCategories = _context.categories.Count();
        ViewData["TotalCategories"] = totalCategories;

        var totalRating=_context.Rating.Count();
        ViewData["totalRating"]= totalRating;

        var highestPriceProduct = _context.Product.OrderByDescending(p => p.Price).FirstOrDefault();
        if (highestPriceProduct != null)
        {
            ViewData["HighestPriceProductName"] = highestPriceProduct.ProductName;
            ViewData["HighestPriceProductPrice"] = highestPriceProduct.Price;
        }
        else
        {
            ViewData["HighestPriceProductName"] = "N/A";
            ViewData["HighestPriceProductPrice"] = 0;
        }

        var mostSoldProduct = _context.Purchase
            .GroupBy(p => p.ProductID)
            .Select(g => new { ProductID = g.Key, TotalSales = g.Count() })
            .OrderByDescending(g => g.TotalSales)
            .FirstOrDefault();

        if (mostSoldProduct != null)
        {
            var mostSoldProductInfo = _context.Product.FirstOrDefault(p => p.ProductID == mostSoldProduct.ProductID);
            if (mostSoldProductInfo != null)
            {
                ViewData["MostSoldProductID"] = mostSoldProductInfo.ProductID;
                ViewData["MostSoldProductName"] = mostSoldProductInfo.ProductName;
                ViewData["MostSoldProductTotalSales"] = mostSoldProduct.TotalSales;
            }
            else
            {
                ViewData["MostSoldProductID"] = "N/A";
                ViewData["MostSoldProductName"] = "N/A";
                ViewData["MostSoldProductTotalSales"] = 0;
            }
        }
        else
        {
            ViewData["MostSoldProductID"] = "N/A";
            ViewData["MostSoldProductName"] = "N/A";
            ViewData["MostSoldProductTotalSales"] = 0;
        }

        var thisYearSales = _context.Purchase
        .Where(p => p.PurchaseDate.Year == DateTime.Now.Year)  // Mevcut yılın satışlarını filtrele
        .GroupBy(p => new { p.PurchaseDate.Month, p.PurchaseDate.Year })
        .Select(g => new SalesData
        {
            Month = g.Key.Month,
            Year = g.Key.Year,
            TotalSales = g.Sum(p => p.Quantity)
        })
        .ToList();

        // ViewModel'e ay isimlerini ekleyin
        var salesDataWithMonthNames = thisYearSales.Select(s => new SalesData
        {
            Month = s.Month,
            MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(s.Month),
            Year = s.Year,
            TotalSales = s.TotalSales
        }).ToList();
        var model = new StatisticsViewModel
        {
            YearlySales = salesDataWithMonthNames,
            
        };

        return View(model);
    }


}

using Microsoft.AspNetCore.Mvc;

namespace Mahsul.Controllers
{
    public class IdentityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

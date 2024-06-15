using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;

namespace Mahsul.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public async Task<IActionResult> Support()
        {
            return View();
        }
            [HttpPost]
        public async Task<IActionResult> Submit(string subject, string message, string email, string PhoneNumber)
        {
            // SMTP ayarlarınızı burada tanımlayın
            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587; // Genellikle 587 veya 465 olur
            string smtpUsername = "amed.alatas1313@gmail.com";
            string smtpPassword = "xxgd vglj xxdx amkq ";
        

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(smtpUsername);
                mail.To.Add("your_email@example.com");
                mail.Subject = subject;
                mail.Body = $"E-posta: {email}\n\n\n\nMesaj: {message}\n\nTelefon:{PhoneNumber}";

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                    try
                    {
                        await smtpClient.SendMailAsync(mail);
                        // E-posta başarıyla gönderildiğinde kullanıcıyı teşekkür sayfasına yönlendir
                        return RedirectToAction("ThankYou");
                    }
                    catch (Exception ex)
                    {
                        // E-posta gönderme başarısız olduysa hata mesajıyla birlikte destek sayfasına geri dön
                        TempData["ErrorMessage"] = "E-posta gönderme hatası: " + ex.Message;
                        return RedirectToAction("ThankYou");
                    }
                }
            }
        }
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}

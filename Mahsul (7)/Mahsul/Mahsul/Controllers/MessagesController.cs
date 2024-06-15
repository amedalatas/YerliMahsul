using Mahsul.Data;
using Mahsul.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mahsul.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // Okunmamış mesaj sayısını hesapla
            var unreadMessageCount = await _context.Messages
                .Where(m => ( m.ReceiverUsername == user.UserName) && !m.IsRead)
                .CountAsync();

            // Aktif kullanıcının username'ine göre gönderici veya alıcı olan tüm mesajları getir
            var messages = await _context.Messages
                .Where(m => m.SenderUserName == user.UserName || m.ReceiverUsername == user.UserName)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // View'a okunmamış mesaj sayısını da gönder
            ViewData["UnreadMessageCount"] = unreadMessageCount;

            return View(messages);
        }



        public IActionResult Create(string receiverId, string ProductName, string ProductCategory,string receiverUsername)
        {
            var model = new MessageCreateViewModel
            {
                ReceiverId = receiverId,
                ReceiverUsername = receiverUsername,
                ProductName = ProductName,
                ProductCategory = ProductCategory
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MessageCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var senderId = _userManager.GetUserId(User);
                var senderUserName = _userManager.GetUserName(User);

                // Fetch the farmer's information
                var farmer = await _context.farmers.FirstOrDefaultAsync(f => f.FarmerID.ToString() == model.ReceiverId);
                if (farmer == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid receiver.");
                    return View(model);
                }

                // Fetch the receiver's username
                var receiver = await _userManager.FindByIdAsync(farmer.UserID);
                if (receiver == null)
                {
                    ModelState.AddModelError(string.Empty, "Receiver not found.");
                    return View(model);
                }

                var message = new Messages
                {
                    SenderId = senderId,
                    SenderUserName = senderUserName,
                    ReceiverId = model.ReceiverId,
                    ReceiverUsername = receiver.UserName, // Assign the fetched username here
                    Content = model.Content,
                    ProductName = model.ProductName,
                    ProductCategory = model.ProductCategory,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }





        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }

            // Sadece alıcı mesajları okunabilir
            if (message.ReceiverUsername != User.Identity.Name)
            {
                return Forbid();
            }

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult ReplyMessage(string senderId, string senderUserName, string productName, string productCategory)
        {
            var model = new MessageCreateViewModel
            {
                ReceiverId = senderId,
                ReceiverUsername = senderUserName,
                ProductName = productName,
                ProductCategory = productCategory
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ReplyMessage(MessageCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var senderId = _userManager.GetUserId(User);
                var senderUserName = _userManager.GetUserName(User);

                var message = new Messages
                {
                    SenderId = senderId,
                    SenderUserName = senderUserName,
                    ReceiverId = model.ReceiverId,
                    ReceiverUsername = model.ReceiverUsername,
                    Content = model.Content,
                    ProductName = model.ProductName,
                    ProductCategory = model.ProductCategory,
                    Timestamp = DateTime.Now,
                    IsRead = false
                };
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
        public async Task<IActionResult> SentMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            var sentMessages = await _context.Messages
                .Where(m => m.SenderId == user.Id)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            return View(sentMessages);
        }

        public async Task<IActionResult> ReceivedMessages()
        {
            var user = await _userManager.GetUserAsync(User);
            var receivedMessages = await _context.Messages
                .Where(m => m.ReceiverUsername == user.UserName)
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();

            // Okunmamış mesaj sayısını hesapla
            var unreadMessageCount = receivedMessages.Count(m => !m.IsRead);
            ViewData["UnreadMessageCount"] = unreadMessageCount;

            return View(receivedMessages);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        // Edit POST method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,SenderId,ReceiverId,SenderUserName,ReceiverUsername,ProductId,ProductName,ProductCategory,Timestamp,IsRead")] Messages message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(message);
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }




    }


}

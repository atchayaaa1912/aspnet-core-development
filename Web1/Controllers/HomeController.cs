using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web1.Data;
using Web1.Models;

namespace Web1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage contactmessage)
        {
            if (!ModelState.IsValid)
            {
                return View(contactmessage);
            }

            _context.ContactMessages.Add(contactmessage);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your message has been submitted successfully!";

            return RedirectToAction("Contact");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages.ToListAsync();

            return View(messages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactMessage contactMessage)
        {
            if (!ModelState.IsValid)
            {
                return View(contactMessage);
            }

            var existingMessage = await _context.ContactMessages
                .FindAsync(contactMessage.Id);

            if (existingMessage == null)
            {
                return NotFound();
            }

            existingMessage.FullName = contactMessage.FullName;
            existingMessage.Email = contactMessage.Email;
            existingMessage.Subject = contactMessage.Subject;
            existingMessage.Message = contactMessage.Message;

            await _context.SaveChangesAsync();

            return RedirectToAction("Messages");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            _context.ContactMessages.Remove(message);

            await _context.SaveChangesAsync();

            return RedirectToAction("Messages");
        }
    }
}

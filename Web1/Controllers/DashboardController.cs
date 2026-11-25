using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Web1.Data;
using Web1.ViewModels;

namespace Web1.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel
            {
                TotalEnquiries = await _context.ContactMessages.CountAsync(),

                TotalDocuments = await _context.Documents.CountAsync(),

                RecentEnquiries = await _context.ContactMessages
                    .OrderByDescending(c => c.SubmittedAt)
                    .Take(5)
                    .ToListAsync(),

                RecentDocuments = await _context.Documents
                    .OrderByDescending(d => d.UploadedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(dashboard);
        }
    }
}
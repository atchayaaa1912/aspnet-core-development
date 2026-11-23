using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web1.Data;
using Web1.Models;
using ClosedXML.Excel;
using System.IO;

namespace Web1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Messages()
        {
            var messages = await _context.ContactMessages.ToListAsync();

            return View(messages);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            // 1. Retrieve all contact messages
            var messages = await _context.ContactMessages
                .OrderBy(m => m.Id)
                .ToListAsync();

            // 2. Create an Excel workbook
            using var workbook = new XLWorkbook();

            // 3. Create a worksheet
            var worksheet = workbook.Worksheets.Add("Contact Messages");

            // 4. Add column headings
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Full Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Subject";
            worksheet.Cell(1, 5).Value = "Message";
            worksheet.Cell(1, 6).Value = "Submitted At";

            // 5. Insert message data
            for (int i = 0; i < messages.Count; i++)
            {
                var row = i + 2;
                var message = messages[i];

                worksheet.Cell(row, 1).Value = message.Id;
                worksheet.Cell(row, 2).Value = message.FullName;
                worksheet.Cell(row, 3).Value = message.Email;
                worksheet.Cell(row, 4).Value = message.Subject;
                worksheet.Cell(row, 5).Value = message.Message;
                worksheet.Cell(row, 6).Value = message.SubmittedAt;
            }

            // 6. Format the worksheet
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Row(1).Style.Fill.BackgroundColor =
                XLColor.LightBlue;

            worksheet.Column(6).Style.DateFormat.Format =
                "dd-MM-yyyy HH:mm:ss";

            worksheet.Columns().AdjustToContents();

            // 7. Save workbook into memory
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            // 8. Return Excel file for download
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ContactMessages.xlsx"
            );
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

            return RedirectToAction(nameof(Messages));
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

            return RedirectToAction(nameof(Messages));
        }

    }
}
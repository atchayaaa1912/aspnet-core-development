
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Web1.Data;
using Web1.Models;
using Web1.Services;

namespace Web1.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DocumentService _documentService;

        public DocumentsController(
            ApplicationDbContext context,
            DocumentService documentService)
        {
            _context = context;
            _documentService = documentService;
        }

        // Display all uploaded documents
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var documents = await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return View(documents);
        }

        // Receive and process an uploaded file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction(nameof(Index));
            }

            // Maximum file size: 5 MB
            const long maxFileSize = 5 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                TempData["ErrorMessage"] =
                    "File size must not exceed 5 MB.";

                return RedirectToAction(nameof(Index));
            }

            // Allow only PDF and JPG/JPEG extensions
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".pdf" &&
                extension != ".jpg" &&
                extension != ".jpeg")
            {
                TempData["ErrorMessage"] =
                    "Only PDF and JPG files are allowed.";

                return RedirectToAction(nameof(Index));
            }
            // Step 1: Calculate file hash
            string contentHash =
                await _documentService.CalculateHashAsync(file);

            // Step 2: Check whether the same content exists
            bool isDuplicate =
                await _documentService.IsDuplicateAsync(contentHash);

            if (isDuplicate)
            {
                TempData["ErrorMessage"] =
                    "This file already exists!";

                return RedirectToAction(nameof(Index));
            }

            // Step 3: Save the physical file
            string storedFileName =
                await _documentService.SaveFileAsync(file);

            // Step 4: Save file information in SQL Server
            var document = new Document
            {
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                ContentHash = contentHash,
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "File uploaded successfully!";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            // Step 1: Find document information in SQL Server
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            // Step 2: Get the physical file path
            string filePath = _documentService.GetFilePath(
                document.StoredFileName
            );

            // Step 3: Check whether the physical file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("The physical file could not be found.");
            }

            // Step 4: Return the file to the browser
            return PhysicalFile(
                filePath,
                "application/octet-stream",
                document.OriginalFileName
            );
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Find the document in SQL Server
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            // Check whether the document exists
            if (document == null)
            {
                return NotFound();
            }

            // Display the confirmation page
            return View(document);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Step 1: Find the document in SQL Server
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            // Step 2: Delete the physical file
            _documentService.DeleteFile(document.StoredFileName);

            // Step 3: Remove the document record from SQL Server
            _context.Documents.Remove(document);

            await _context.SaveChangesAsync();

            // Step 4: Display success message
            TempData["SuccessMessage"] =
                "Document deleted successfully!";

            return RedirectToAction(nameof(Index));
        }


    }
}

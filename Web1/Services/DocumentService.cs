
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using Web1.Data;
using Microsoft.EntityFrameworkCore;

namespace Web1.Services
{
    public class DocumentService
    {
        private readonly string _uploadFolder;
        private readonly ApplicationDbContext _context;

        public DocumentService(IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _uploadFolder = Path.Combine(
                environment.ContentRootPath,
                "UploadedFiles"
            );
            _context = context;
        }

        // Calculate SHA-256 hash of the uploaded file
        public async Task<string> CalculateHashAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            byte[] hashBytes = await SHA256.HashDataAsync(stream);

            return Convert.ToHexString(hashBytes);
        }

        public async Task<bool> IsDuplicateAsync(string contentHash)
        {
            return await _context.Documents
                .AnyAsync(d => d.ContentHash == contentHash);
        }
        // Save the uploaded file to the server
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            Directory.CreateDirectory(_uploadFolder);

            // Generate a unique name instead of using the user's filename
            string storedFileName = Guid.NewGuid().ToString("N");

            string filePath = Path.Combine(
                _uploadFolder,
                storedFileName
            );

            using var fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write
            );

            await file.CopyToAsync(fileStream);

            return storedFileName;
        }

        public string GetFilePath(string storedFileName)
        {
            return Path.Combine(_uploadFolder, storedFileName);
        }

        public void DeleteFile(string storedFileName)
        {
            string filePath = GetFilePath(storedFileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}


using System.ComponentModel.DataAnnotations;

namespace Web1.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [Required]
        [StringLength(64)]
        public string ContentHash { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

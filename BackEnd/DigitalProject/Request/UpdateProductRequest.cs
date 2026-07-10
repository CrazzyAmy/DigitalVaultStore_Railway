using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class UpdateProductRequest
    {
        [Required]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "價格必須大於 0")]
        public decimal Price { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public string? ThumbnailUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public bool IsPublished { get; set; }
    }
}

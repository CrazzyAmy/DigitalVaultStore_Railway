using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class CreateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
        public bool IsVisible { get; set; } = true;
    }
}

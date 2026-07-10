namespace DigitalProject.Models
{
    public partial class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; } = null!;

        public string? Description { get; set; }

        public int SortOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}

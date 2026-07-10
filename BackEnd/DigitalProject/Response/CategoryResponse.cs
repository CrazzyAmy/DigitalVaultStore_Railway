namespace DigitalProject.Response
{
    public class CategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int ProductCount { get; set; }
        public bool IsVisible { get; set; }  // ← 新增
    }
}

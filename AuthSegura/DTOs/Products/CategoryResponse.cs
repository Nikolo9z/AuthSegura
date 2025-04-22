namespace AuthSegura.DTOs.Products
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public List<CategoryResponse> SubCategories { get; set; } = new List<CategoryResponse>();
    }
}

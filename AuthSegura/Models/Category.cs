namespace AuthSegura.Models
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Relación jerárquica
        public int? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        
        // Productos en esta categoría
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

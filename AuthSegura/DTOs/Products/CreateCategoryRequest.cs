namespace AuthSegura.DTOs.Products
{
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}
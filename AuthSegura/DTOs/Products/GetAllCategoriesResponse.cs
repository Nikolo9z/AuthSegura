namespace AuthSegura.DTOs.Products
{
    public class GetAllCategoriesResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool HasChildren { get; set; }
    }
}

namespace AuthSegura.DTOs.Products
{
    public class GetAllProductsResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Category { get; set; }
        public string? CategoryName { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal finalPrice { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public bool IsDiscountActive { get; set; }
    }
}

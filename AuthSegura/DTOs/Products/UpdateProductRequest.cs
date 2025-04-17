namespace AuthSegura.DTOs.Products
{
    public class UpdateProductRequest
    {
        public required int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
    }
}

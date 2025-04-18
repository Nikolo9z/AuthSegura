public class CreateProductResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
    public required string Description { get; set; }
    public required int Stock { get; set; }
    public required string ImageUrl { get; set; }
    public string Category { get; set; }
}
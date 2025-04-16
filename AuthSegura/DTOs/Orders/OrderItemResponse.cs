public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
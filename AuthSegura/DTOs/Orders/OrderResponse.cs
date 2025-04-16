public class OrderResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string UserName { get; set; }
    public DateTime OrderDate { get; set; }
    public required decimal TotalAmount { get; set; }
    public required List<OrderItemResponse> OrderItems { get; set; }
}
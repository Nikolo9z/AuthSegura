public class OrderRequest
{
    public required int userId { get; set; }
    public required List<OrderItemRequest> items { get; set; }
}
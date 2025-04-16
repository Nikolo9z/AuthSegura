public interface IOrderService
{
    Task<Order> GetOrderByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<OrderResponse> CreateOrderAsync(OrderRequest order);
    Task<Order> UpdateOrderAsync(int id, Order order);
    Task<bool> DeleteOrderAsync(int id);
}
public interface IOrderService
{
    Task<OrderResponse> GetOrderByIdAsync(int id);
    Task<OrderResponse[]> GetAllOrdersAsync();
    Task<OrderResponse> CreateOrderAsync(OrderRequest order);
    Task<OrderResponse[]> GetOrdersByUser(int userId);
    Task<OrderResponse[]> GetFilterOrderByDate (DateTime startDate, DateTime endDate);
    Task<OrderResponse[]> GetFilterOrderByUserByDate (int userId, DateTime startDate, DateTime endDate);
}
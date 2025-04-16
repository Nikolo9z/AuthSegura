using AuthSegura.DataAccess;
using AuthSegura.Models;

public class OrderService : IOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IProductService _productService;

    public OrderService(AppDbContext dbContext, IProductService productService)
    {
        _dbContext = dbContext;
        _productService = productService;
    }

    public async Task<OrderResponse> CreateOrderAsync(OrderRequest order)

    {
        await ValidateOrderRequestAsync(order);
        var user = await _dbContext.Users.FindAsync(order.userId) ?? throw new ArgumentException("User not found", nameof(order.userId));
        var total = await CalculateTotalAsync(order.items);
        var productsDict = new Dictionary<int, Product>();
        foreach (var item in order.items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (!productsDict.ContainsKey(item.ProductId))
                productsDict[item.ProductId] = product;
        }
        var newOrder = new Order
        {
            UserId = order.userId,
            User = user,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            OrderItems = new List<OrderItem>()
        };
        _dbContext.Orders.Add(newOrder);
        foreach (var item in order.items)
        {
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Order = newOrder,
                Product = productsDict[item.ProductId]
            };
            newOrder.OrderItems.Add(orderItem);
        }
        await _dbContext.SaveChangesAsync();
        return new OrderResponse
        {
            Id = newOrder.Id,
            UserId = newOrder.UserId,
            OrderDate = newOrder.OrderDate,
            TotalAmount = newOrder.TotalAmount,
            UserName = newOrder.User.Username,
            OrderItems = order.items.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity
            }).ToList()
        };
    }
    Task<bool> IOrderService.DeleteOrderAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task<IEnumerable<Order>> IOrderService.GetAllOrdersAsync()
    {
        throw new NotImplementedException();
    }

    Task<Order> IOrderService.GetOrderByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    Task<Order> IOrderService.UpdateOrderAsync(int id, Order order)
    {
        throw new NotImplementedException();
    }
    private async Task ValidateOrderRequestAsync(OrderRequest order)
    {
    if (order == null)
        throw new ArgumentNullException(nameof(order));
    if (order.userId <= 0)
        throw new ArgumentException("User ID must be greater than zero", nameof(order.userId));
    if (string.IsNullOrWhiteSpace(order.userId.ToString()))
        throw new ArgumentException("User ID is required", nameof(order.userId));
    if (order.items == null || order.items.Count == 0)
        throw new ArgumentException("Order items are required", nameof(order.items));

    foreach (var item in order.items)
    {
        var product = await _productService.GetProductByIdAsync(item.ProductId);
        if (product == null)
            throw new ArgumentException($"Product with ID {item.ProductId} not found");
        if (item.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");
    }
    }
    private async Task<decimal> CalculateTotalAsync(List<OrderItemRequest> items)
{
    decimal total = 0;
    foreach (var item in items)
    {
        var product = await _productService.GetProductByIdAsync(item.ProductId);
        total += product.Price * item.Quantity;
    }
    return total;
}


    
}

using AuthSegura.DataAccess;
using AuthSegura.Models;
using Microsoft.EntityFrameworkCore;

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
        var productsDict = new Dictionary<int, Product>();
        
        // Cargamos todos los productos primero
        foreach (var item in order.items)
        {
            var product = await _dbContext.Products.FindAsync(item.ProductId) ?? throw new ArgumentException("Product not found", nameof(item.ProductId));
            if (!productsDict.ContainsKey(item.ProductId))
                productsDict[item.ProductId] = product;
        }
        
        var total = await CalculateTotalAsync(order.items);
        
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
            var product = productsDict[item.ProductId];
            var finalPrice = product.GetFinalPrice(); // Usa el método que calcula el precio con descuento
            
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Order = newOrder,
                Product = product,
                UnitPrice = product.Price, // Precio original
                DiscountedPrice = finalPrice, // Precio con descuento aplicado
                DiscountPercentage = product.DiscountPercentage // Porcentaje de descuento aplicado (si existe)
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
            OrderItems = newOrder.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage
            }).ToList()
        };
    }

    async Task<OrderResponse[]> IOrderService.GetAllOrdersAsync()
    {
        var orders = await  _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();
        return orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            UserName = o.User.Username,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            OrderItems = o.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
            }).ToList()
        }).ToArray();
    }

    async Task<OrderResponse> IOrderService.GetOrderByIdAsync(int id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Order with ID {id} not found.");
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            UserName = order.User.Username,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            OrderItems = order.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
            }).ToList()
        };
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
        // Usar el precio final que ya incluye el descuento si está activo
        total += product.FinalPrice * item.Quantity;
    }
    return total;
}

    public async Task<OrderResponse[]> GetOrdersByUser(int userId)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .ToListAsync();
        return orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            UserName = o.User.Username,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            OrderItems = o.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
            }).ToList()
        }).ToArray();
    }

    public async Task<OrderResponse[]> GetFilterOrderByDate(DateTime startDate, DateTime endDate)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .ToListAsync();
        return orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            UserName = o.User.Username,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            OrderItems = o.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
            }).ToList()
        }).ToArray();
    }

    public async Task<OrderResponse[]> GetFilterOrderByUserByDate(int userId, DateTime startDate, DateTime endDate)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId && o.OrderDate >= startDate && o.OrderDate <= endDate)
            .ToListAsync();
        return orders.Select(o => new OrderResponse
        {
            Id = o.Id,
            UserId = o.UserId,
            UserName = o.User.Username,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            OrderItems = o.OrderItems.Select(item => new OrderItemResponse
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
            }).ToList()
        }).ToArray();
    }
}

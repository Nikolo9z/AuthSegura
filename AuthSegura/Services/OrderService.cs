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

    public async Task<OrderResponse> CreateOrderAsync(OrderItemRequest[] order, int userId)
    {
        await ValidateOrderRequestAsync(order);
        var user = await _dbContext.Users.FindAsync(userId) ?? throw new ArgumentException("User not found", nameof(userId));
        var productsDict = new Dictionary<int, Product>();
        
        // Cargamos todos los productos primero
        foreach (var item in order)
        {
            var product = await _dbContext.Products.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId) ?? throw new ArgumentException($"Product with ID {item.ProductId} not found", nameof(item.ProductId));
            
            // Verificar si hay suficiente stock disponible
            if (product.Stock < item.Quantity)
                throw new InvalidOperationException($"No hay suficiente stock disponible para el producto {product.Name}. Stock disponible: {product.Stock}");
                
            if (!productsDict.ContainsKey(item.ProductId))
                productsDict[item.ProductId] = product;
        }
        
        var total = await CalculateTotalAsync(order);
        
        var newOrder = new Order
        {
            UserId = userId,
            User = user,
            OrderDate = DateTime.UtcNow,
            TotalAmount = total,
            OrderItems = new List<OrderItem>()
        };
        
        _dbContext.Orders.Add(newOrder);
        
        foreach (var item in order)
        {
            var product = productsDict[item.ProductId];
            var finalPrice = product.GetFinalPrice(); // Usa el método que calcula el precio con descuento
            
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Order = newOrder,
                Product = product,
                UnitPrice = product.Price // Precio original
            };
            
            // Verificar si hay un descuento válido y aplicarlo
            if (product.IsDiscountActive())
            {
                orderItem.DiscountedPrice = finalPrice; // Precio con descuento aplicado
                orderItem.DiscountPercentage = product.DiscountPercentage; // Porcentaje de descuento aplicado
            }
            else
            {
                orderItem.DiscountedPrice = product.Price; // Sin descuento, precio normal
                orderItem.DiscountPercentage = null; // Sin descuento
            }
            
            // Actualizar el stock del producto
            product.Stock -= item.Quantity;
            _dbContext.Products.Update(product);
            
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
                CategoryId =item.Product.Category.Id,
                Category= item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl,
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
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage,
                CategoryId = item.Product.Category.Id,
                Category = item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl
            }).ToList()
        }).ToArray();
    }

    async Task<OrderResponse> IOrderService.GetOrderByIdAsync(int id)
    {
        var order = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
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
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage,
                CategoryId = item.Product.Category.Id,
                Category = item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl
            }).ToList()
        };
    }
    private async Task ValidateOrderRequestAsync(OrderItemRequest[] order)
    {
    if (order == null)
        throw new ArgumentNullException(nameof(order));
    if (order.Length == 0)
        throw new ArgumentException("Order cannot be empty", nameof(order));

    foreach (var item in order)
    {
        var product = await _productService.GetProductByIdAsync(item.ProductId);
        if (product == null)
            throw new ArgumentException($"Product with ID {item.ProductId} not found");
        if (item.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");
    }
    }
    private async Task<decimal> CalculateTotalAsync(OrderItemRequest[] items)
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
                .ThenInclude(p => p.Category)
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
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage,
                CategoryId = item.Product.Category.Id,
                Category = item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl
            }).ToList()
        }).ToArray();
    }

    public async Task<OrderResponse[]> GetFilterOrderByDate(DateTime startDate, DateTime endDate)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
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
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage,
                CategoryId = item.Product.Category.Id,
                Category = item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl
            }).ToList()
        }).ToArray();
    }

    public async Task<OrderResponse[]> GetFilterOrderByUserByDate(int userId, DateTime startDate, DateTime endDate)
    {
        var orders = await _dbContext.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
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
                Price = item.UnitPrice,
                DiscountedPrice = item.DiscountedPrice,
                DiscountPercentage = item.DiscountPercentage,
                CategoryId = item.Product.Category.Id,
                Category = item.Product.Category.Name,
                ProductDescription = item.Product.Description,
                ProductImage = item.Product.ImageUrl

            }).ToList()
        }).ToArray();
    }
}

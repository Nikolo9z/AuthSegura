using System.Data.Entity;
using AuthSegura.DataAccess;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _config;

    public ProductService(AppDbContext dbContext, IConfiguration config)
    {
        _dbContext = dbContext;
        _config = config;
    }
        public async Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (string.IsNullOrEmpty(product.Name)) throw new ArgumentException("Product name is required", nameof(product.Name));
        if (product.Price <= 0) throw new ArgumentException("Product price must be greater than zero", nameof(product.Price));
        if (product.Stock < 0) throw new ArgumentException("Product stock cannot be negative", nameof(product.Stock));
        var newProduct = new Product
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync();
        return new CreateProductResponse
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Price = newProduct.Price,
            Description = newProduct.Description,
            Stock = newProduct.Stock,
            ImageUrl = newProduct.ImageUrl
        };
    }

    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products.FindAsync(id) ?? throw new KeyNotFoundException($"Product with ID {id} not found.");
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }
    
    public async Task<Product> UpdateProductAsync(int id, Product product)
    {
        var existingProduct = await GetProductByIdAsync(id);
        if (existingProduct == null) throw new KeyNotFoundException($"Product with ID {id} not found.");

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;
        existingProduct.Description = product.Description;
        existingProduct.Stock = product.Stock;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        existingProduct.ImageUrl = product.ImageUrl;
        await _dbContext.SaveChangesAsync();
        return existingProduct;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await GetProductByIdAsync(id);
        if (product == null) return false;

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
using AuthSegura.DataAccess;
using AuthSegura.DTOs.Products;
using Microsoft.EntityFrameworkCore;

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

    public async Task<GetProductByIdResponse> GetProductByIdAsync(int id)
    {
        var product = await _dbContext.Products.FindAsync(id) ?? throw new KeyNotFoundException($"Product with ID {id} not found.");
        return new GetProductByIdResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl
        };

    }

    public async Task<GetAllProductsResponse[]> GetAllProductsAsync()
    {

        var products = await _dbContext.Products.ToListAsync();
        return products.Select(p => new GetAllProductsResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl
        }).ToArray();
    }

    public async Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request)
    {
        var existingProduct = await _dbContext.Products.FindAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        if (existingProduct == null) throw new KeyNotFoundException($"Product with ID {request.Id} not found.");
        if (!string.IsNullOrEmpty(request.Name)) existingProduct.Name = request.Name;
        if (request.Price > 0) existingProduct.Price = request.Price;
        if (!string.IsNullOrEmpty(request.Description)) existingProduct.Description = request.Description;
        if (request.Stock >= 0) existingProduct.Stock = request.Stock;
        if (!string.IsNullOrEmpty(request.ImageUrl)) existingProduct.ImageUrl = request.ImageUrl;
        existingProduct.UpdatedAt = DateTime.UtcNow;
        _dbContext.Products.Update(existingProduct);
        await _dbContext.SaveChangesAsync();
        return new UpdateProductResponse
        {
            Id = existingProduct.Id,
            Name = existingProduct.Name,
            Price = existingProduct.Price,
            Description = existingProduct.Description,
            Stock = existingProduct.Stock,
            ImageUrl = existingProduct.ImageUrl
        };
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);
        if (product == null) return false;
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
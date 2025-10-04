using AuthSegura.DataAccess;
using AuthSegura.DTOs.Products;
using AuthSegura.Models;
using AuthSegura.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ProductService : IProductService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly ICategoryService _categoryService;

    public ProductService(AppDbContext dbContext, IConfiguration config, ICategoryService categoryService)
    {
        _dbContext = dbContext;
        _config = config;
        _categoryService = categoryService;
    }
    
    public async Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product)
    {
        if (product == null) throw new ArgumentNullException(nameof(product));
        if (string.IsNullOrEmpty(product.Name)) throw new ArgumentException("Product name is required", nameof(product.Name));
        if (product.Price <= 0) throw new ArgumentException("Product price must be greater than zero", nameof(product.Price));
        if (product.Stock < 0) throw new ArgumentException("Product stock cannot be negative", nameof(product.Stock));

        // Validar el descuento si está presente
        if (product.DiscountPercentage.HasValue)
        {
            if (product.DiscountPercentage < 0 || product.DiscountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(product.DiscountPercentage));

            // Si hay descuento, las fechas son obligatorias
            if (!product.DiscountStartDate.HasValue)
                throw new ArgumentException("Discount start date is required when discount is applied", nameof(product.DiscountStartDate));
            if (!product.DiscountEndDate.HasValue)
                throw new ArgumentException("Discount end date is required when discount is applied", nameof(product.DiscountEndDate));
            if (product.DiscountEndDate < product.DiscountStartDate)
                throw new ArgumentException("Discount end date cannot be earlier than start date", nameof(product.DiscountEndDate));
        }
        var newProduct = new Product
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CategoryId = product.CategoryId,
            Category = await _dbContext.Categories.FindAsync(product.CategoryId) ?? throw new KeyNotFoundException($"Category with ID {product.CategoryId} not found."),
            DiscountPercentage = product.DiscountPercentage,
            DiscountStartDate = product.DiscountStartDate,
            DiscountEndDate = product.DiscountEndDate
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
            ImageUrl = newProduct.ImageUrl,
            Category = newProduct.Category.Name,
            DiscountPercentage = newProduct.DiscountPercentage,
            DiscountStartDate = newProduct.DiscountStartDate,
            DiscountEndDate = newProduct.DiscountEndDate,
            FinalPrice = newProduct.GetFinalPrice()
        };
    }

    public async Task<GetProductByIdResponse> GetProductByIdAsync(int id)
    {
        var product = await _dbContext.Products.Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product with ID {id} not found.");
        return new GetProductByIdResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Description = product.Description,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            DiscountPercentage = product.DiscountPercentage,
            DiscountStartDate = product.DiscountStartDate,
            DiscountEndDate = product.DiscountEndDate,
            FinalPrice = product.GetFinalPrice(),
            Category = product.CategoryId,
            CategoryName= product.Category.Name,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            isDiscountActive= product.IsDiscountActive()
        };

    }

    public async Task<GetAllProductsResponse[]> GetAllProductsAsync()
    {

        var products = await _dbContext.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .ToListAsync();
        return products.Select(p => new GetAllProductsResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            Category = p.CategoryId,
            CategoryName = p.Category.Name,
            DiscountPercentage = p.DiscountPercentage,
            finalPrice = p.GetFinalPrice(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            DiscountStartDate = p.DiscountStartDate,
            DiscountEndDate = p.DiscountEndDate,
            IsDiscountActive = p.IsDiscountActive()
        }).ToArray();
    }

    public async Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request)
    {
        var existingProduct = await _dbContext.Products.FindAsync(request.Id)
            ?? throw new KeyNotFoundException($"Product with ID {request.Id} not found.");

        if (!string.IsNullOrEmpty(request.Name)) existingProduct.Name = request.Name;
        if (request.Price > 0) existingProduct.Price = request.Price;
        if (!string.IsNullOrEmpty(request.Description)) existingProduct.Description = request.Description;
        if (request.Stock >= 0) existingProduct.Stock = request.Stock;
        if (!string.IsNullOrEmpty(request.ImageUrl)) existingProduct.ImageUrl = request.ImageUrl;
        if (request.CategoryId > 0)
        {
            var category = await _dbContext.Categories.FindAsync(request.CategoryId)
                ?? throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");
            existingProduct.CategoryId = request.CategoryId;
            existingProduct.Category = category;
        }

        if (request.RemoveDiscount)
        {
            // Opcional: si se indica explícitamente eliminar el descuento
            existingProduct.DiscountPercentage = null;
            existingProduct.DiscountStartDate = null;
            existingProduct.DiscountEndDate = null;
        }
        else if (request.DiscountPercentage.HasValue)
        {
            if (request.DiscountPercentage < 0 || request.DiscountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(request.DiscountPercentage));

            existingProduct.DiscountPercentage = request.DiscountPercentage;

            // Si hay descuento, las fechas son obligatorias
            if (request.DiscountStartDate.HasValue)
                existingProduct.DiscountStartDate = request.DiscountStartDate;
            else if (!existingProduct.DiscountStartDate.HasValue)
                throw new ArgumentException("Discount start date is required when discount is applied", nameof(request.DiscountStartDate));

            if (request.DiscountEndDate.HasValue)
                existingProduct.DiscountEndDate = request.DiscountEndDate;
            else if (!existingProduct.DiscountEndDate.HasValue)
                throw new ArgumentException("Discount end date is required when discount is applied", nameof(request.DiscountEndDate));

            if ((existingProduct.DiscountEndDate < existingProduct.DiscountStartDate))
                throw new ArgumentException("Discount end date cannot be earlier than start date", nameof(request.DiscountEndDate));
        }


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
            ImageUrl = existingProduct.ImageUrl,
            DiscountPercentage = existingProduct.DiscountPercentage,
            DiscountStartDate = existingProduct.DiscountStartDate,
            DiscountEndDate = existingProduct.DiscountEndDate,
            FinalPrice = existingProduct.GetFinalPrice(),
            CategoryId= existingProduct.CategoryId,
            CategoryName = existingProduct.Category.Name,

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
    
    public async Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId)
    {
        var products = await _dbContext.Products
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Id)
            .ToListAsync();
        
        return products.Select(p => new GetAllProductsResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Description = p.Description,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            Category = p.CategoryId,
            finalPrice = p.GetFinalPrice(),
            DiscountPercentage = p.DiscountPercentage,
            DiscountStartDate = p.DiscountStartDate,
            DiscountEndDate = p.DiscountEndDate,
            IsDiscountActive = p.IsDiscountActive()
        }).ToArray();
    }

    // Obtener productos por categoría con opción para incluir subcategorías
    public async Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId, bool includeSubcategories = false)
    {
        if (!includeSubcategories)
        {
            // Comportamiento existente: solo productos de esta categoría
            var products = await _dbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Id)
                .ToListAsync();

            return products.Select(p => new GetAllProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                Category = categoryId,
                finalPrice = p.GetFinalPrice(),
                DiscountPercentage = p.DiscountPercentage,
                DiscountStartDate = p.DiscountStartDate,
                DiscountEndDate = p.DiscountEndDate,
                IsDiscountActive = p.IsDiscountActive()
            }).ToArray();
        }
        else
        {
            // Nueva funcionalidad: incluir productos de las subcategorías
            var categoryIds = new HashSet<int> { categoryId };
            await CollectSubcategoryIds(categoryId, categoryIds);

            var products = await _dbContext.Products
                .Where(p => categoryIds.Contains(p.CategoryId))
                .OrderBy(p => p.Id)
                .ToListAsync();

            return products.Select(p => new GetAllProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                Category = p.CategoryId,
                finalPrice = p.GetFinalPrice(),
                DiscountPercentage = p.DiscountPercentage,
                DiscountStartDate = p.DiscountStartDate,
                DiscountEndDate = p.DiscountEndDate,
                IsDiscountActive = p.IsDiscountActive()
            }).ToArray();
        }
    }

    // Método auxiliar para recopilar todos los IDs de subcategorías recursivamente
    private async Task CollectSubcategoryIds(int categoryId, HashSet<int> categoryIds)
    {
        var subcategories = await _dbContext.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (var subCategoryId in subcategories)
        {
            if (categoryIds.Add(subCategoryId))
            {
                await CollectSubcategoryIds(subCategoryId, categoryIds);
            }
        }
    }
}
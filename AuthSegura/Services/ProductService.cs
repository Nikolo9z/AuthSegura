using AuthSegura.DataAccess;
using AuthSegura.DTOs.Products;
using AuthSegura.Models;
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
            UpdatedAt = DateTime.UtcNow,
            CategoryId = product.CategoryId,
            Category= await _dbContext.Categories.FindAsync(product.CategoryId) ?? throw new KeyNotFoundException($"Category with ID {product.CategoryId} not found.")
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
    public async Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId)
    {
        var products = await _dbContext.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
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

    public async Task<CategoryResponse> CreateCategory(string name)
    {
        var newCategory = new Category
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Categories.Add(newCategory);
        await _dbContext.SaveChangesAsync();
        return new CategoryResponse
        {
            Id = newCategory.Id,
            Name = newCategory.Name,
            CreatedAt = newCategory.CreatedAt,
            UpdatedAt = newCategory.UpdatedAt
        };


    }

    public async Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request)
    {
        var category = await _dbContext.Categories.FindAsync(request.Id)
            ?? throw new KeyNotFoundException($"Category with ID {request.Id} not found.");
        category.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(request.Name)) category.Name = request.Name;
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync();
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    // Nuevo método para crear categoría con jerarquía
    public async Task<CategoryResponse> CreateCategory(CreateCategoryRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
            throw new ArgumentException("Category name is required", nameof(request.Name));
        
        var newCategory = new Category
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ParentCategoryId = request.ParentCategoryId
        };
        
        // Verifica que la categoría padre exista si se proporciona
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _dbContext.Categories.FindAsync(request.ParentCategoryId)
                ?? throw new KeyNotFoundException($"Parent category with ID {request.ParentCategoryId} not found");
            newCategory.ParentCategory = parentCategory;
        }
        
        _dbContext.Categories.Add(newCategory);
        await _dbContext.SaveChangesAsync();
        
        return new CategoryResponse
        {
            Id = newCategory.Id,
            Name = newCategory.Name,
            CreatedAt = newCategory.CreatedAt,
            UpdatedAt = newCategory.UpdatedAt,
            ParentCategoryId = newCategory.ParentCategoryId,
            ParentCategoryName = newCategory.ParentCategory?.Name
        };
    }

    // Obtener todas las categorías planas
    public async Task<GetAllCategoriesResponse[]> GetAllCategoriesFlatAsync()
    {
        var categories = await _dbContext.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .ToListAsync();
            
        return categories.Select(c => new GetAllCategoriesResponse
        {
            Id = c.Id,
            Name = c.Name,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name,
            HasChildren = c.SubCategories.Any()
        }).ToArray();
    }

    // Obtener todas las categorías en formato jerárquico
    public async Task<GetAllCategoriesResponse[]> GetAllCategoriesAsync()
    {
        var allCategories = await GetAllCategoriesFlatAsync();
        
        // Devuelve solo las categorías de nivel superior para evitar redundancia
        return allCategories.Where(c => c.ParentCategoryId == null).ToArray();
    }

    // Obtener una categoría por ID con sus subcategorías
    public async Task<CategoryResponse> GetCategoryByIdAsync(int id)
    {
        var category = await _dbContext.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found");
        
        var response = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name
        };
        
        // Agregar subcategorías recursivamente
        foreach (var subCategory in category.SubCategories)
        {
            var subCategoryResponse = await GetCategoryByIdAsync(subCategory.Id);
            response.SubCategories.Add(subCategoryResponse);
        }
        
        return response;
    }

    // Obtener subcategorías directas para una categoría
    public async Task<CategoryResponse[]> GetSubcategoriesAsync(int categoryId)
    {
        var category = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId)
            ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");
            
        var result = new List<CategoryResponse>();
        foreach (var subCategory in category.SubCategories)
        {
            result.Add(await GetCategoryByIdAsync(subCategory.Id));
        }
        
        return result.ToArray();
    }

    // Obtener categorías raíz
    public async Task<CategoryResponse[]> GetRootCategoriesAsync()
    {
        var rootCategories = await _dbContext.Categories
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();
            
        var result = new List<CategoryResponse>();
        foreach (var category in rootCategories)
        {
            result.Add(await GetCategoryByIdAsync(category.Id));
        }
        
        return result.ToArray();
    }

    // Obtener productos por categoría con opción para incluir subcategorías
    public async Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId, bool includeSubcategories = false)
    {
        if (!includeSubcategories)
        {
            // Comportamiento existente: solo productos de esta categoría
            var products = await _dbContext.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
                
            return products.Select(p => new GetAllProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                Category = categoryId
            }).ToArray();
        }
        else
        {
            // Nueva funcionalidad: incluir productos de las subcategorías
            var categoryIds = new HashSet<int> { categoryId };
            await CollectSubcategoryIds(categoryId, categoryIds);
            
            var products = await _dbContext.Products
                .Where(p => categoryIds.Contains(p.CategoryId))
                .ToListAsync();
                
            return products.Select(p => new GetAllProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                Category = p.CategoryId
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
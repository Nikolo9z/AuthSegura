using AuthSegura.DataAccess;
using AuthSegura.DTOs.Products;
using AuthSegura.Models;
using AuthSegura.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _dbContext;

    public CategoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
            UpdatedAt = category.UpdatedAt,
        };
    }

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

    public async Task<GetAllCategoriesResponse[]> GetAllCategoriesAsync()
    {
        var allCategories = await GetAllCategoriesFlatAsync();

        // Devuelve solo las categorías de nivel superior para evitar redundancia
        return allCategories.Where(c => c.ParentCategoryId == null).ToArray();
    }

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

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return false;
        var subcategories = await _dbContext.Categories
            .Where(c => c.ParentCategoryId == id)
            .ToListAsync();

        foreach (var subcategory in subcategories)
        {
            _dbContext.Categories.Remove(subcategory);
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return true;
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
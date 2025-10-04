using AuthSegura.DTOs.Products;

public interface ICategoryService
{
    Task<CategoryResponse> CreateCategory(string name);
    Task<CategoryResponse> CreateCategory(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request);
    Task<GetAllCategoriesResponse[]> GetAllCategoriesFlatAsync();
    Task<GetAllCategoriesResponse[]> GetAllCategoriesAsync();
    Task<CategoryResponse> GetCategoryByIdAsync(int id);
    Task<CategoryResponse[]> GetSubcategoriesAsync(int categoryId);
    Task<CategoryResponse[]> GetRootCategoriesAsync();
    Task<bool> DeleteCategoryAsync(int id);
}
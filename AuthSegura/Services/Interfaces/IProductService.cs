using AuthSegura.DTOs.Products;

public interface IProductService
{
    Task<GetProductByIdResponse> GetProductByIdAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsAsync();
    Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product);
    Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    
    // Métodos actualizados y nuevos para categorías
    Task<GetAllCategoriesResponse[]> GetAllCategoriesAsync();
    Task<GetAllCategoriesResponse[]> GetAllCategoriesFlatAsync();
    Task<CategoryResponse> GetCategoryByIdAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId, bool includeSubcategories = false);
    Task<CategoryResponse> CreateCategory(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request);
    Task<CategoryResponse[]> GetSubcategoriesAsync(int categoryId);
    Task<CategoryResponse[]> GetRootCategoriesAsync();
    Task<bool> DeleteCategoryAsync(int id);
}
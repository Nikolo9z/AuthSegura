using AuthSegura.DTOs.Products;

public interface IProductService
{
    Task<GetProductByIdResponse> GetProductByIdAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsAsync();
    Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product);
    Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<GetAllCategoriesResponse[]> GetAllCategoriesAsync();
    Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId);
    Task<CategoryResponse> CreateCategory(string name);
    Task<CategoryResponse> UpdateCategoryAsync(UpdateCategoryRequest request);

}
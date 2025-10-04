using AuthSegura.DTOs.Products;

public interface IProductService {
    Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product);
    Task<GetProductByIdResponse> GetProductByIdAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsAsync();
    Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId);
    Task<GetAllProductsResponse[]> GetAllProductsByCategory(int categoryId, bool includeSubcategories);
}
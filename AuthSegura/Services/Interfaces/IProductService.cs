using AuthSegura.DTOs.Products;

public interface IProductService
{
    Task<GetProductByIdResponse> GetProductByIdAsync(int id);
    Task<GetAllProductsResponse[]> GetAllProductsAsync();
    Task<CreateProductResponse> CreateProductAsync(CreateProductRequest product);
    Task<UpdateProductResponse> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
}
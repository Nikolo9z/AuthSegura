using AuthSegura.DTOs.Products;
using AuthSegura.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSegura.Controllers
{
    [Route("[controller]")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("create")]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _productService.CreateProductAsync(request);
                return Ok(ApiResponse<CreateProductResponse>.Ok(response, "Producto creado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _productService.UpdateProductAsync(request);
                return Ok(ApiResponse<UpdateProductResponse>.Ok(response, "Producto actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var response = await _productService.GetProductByIdAsync(id);
                return Ok(ApiResponse<GetProductByIdResponse>.Ok(response));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _productService.GetAllProductsAsync();
                return Ok(ApiResponse<GetAllProductsResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles="admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _productService.DeleteProductAsync(id);
                if (!response)
                {
                    return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));
                }
                return Ok(ApiResponse<bool>.Ok(response, "Producto eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("getallbycategory/{categoryId}")]
        public async Task<IActionResult> GetAllByCategory(int categoryId)
        {
            try
            {
                var response = await _productService.GetAllProductsByCategory(categoryId);
                return Ok(ApiResponse<GetAllProductsResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("getallbycategory/{categoryId}/{includeSubcategories?}")]
        public async Task<IActionResult> GetAllByCategory(int categoryId, bool includeSubcategories = false)
        {
            try
            {
                var response = await _productService.GetAllProductsByCategory(categoryId, includeSubcategories);
                return Ok(ApiResponse<GetAllProductsResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}

using AuthSegura.DTOs.Products;
using AuthSegura.Helpers;
using AuthSegura.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSegura.Controllers
{
    [ApiController]
    [Route("categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // [HttpGet]
        // public async Task<IActionResult> GetAllCategories()
        // {
        //     var categories = await _categoryService.GetAllCategoriesAsync();
        //     return Ok(ApiResponse<GetAllCategoriesResponse[]>.Ok(categories));
        // }

        // [HttpGet("flat")]
        // public async Task<IActionResult> GetAllCategoriesFlat()
        // {
        //     var categories = await _categoryService.GetAllCategoriesFlatAsync();
        //     return Ok(ApiResponse<GetAllCategoriesResponse[]>.Ok(categories));
        // }

        [HttpGet("root")]
        public async Task<IActionResult> GetRootCategories()
        {
            var categories = await _categoryService.GetRootCategoriesAsync();
            return Ok(ApiResponse<CategoryResponse[]>.Ok(categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return Ok(ApiResponse<CategoryResponse>.Ok(category));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/subcategories")]
        public async Task<IActionResult> GetSubcategories(int id)
        {
            try
            {
                var subcategories = await _categoryService.GetSubcategoriesAsync(id);
                return Ok(ApiResponse<CategoryResponse[]>.Ok(subcategories));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CategoryResponse[]>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.CreateCategory(request);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, ApiResponse<CategoryResponse>.Ok(category));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(request);
                return Ok(ApiResponse<CategoryResponse>.Ok(category));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<CategoryResponse>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result)
                return Ok(ApiResponse<bool>.Ok(true, "Category deleted successfully"));
            return NotFound(ApiResponse<bool>.Fail("Category not found"));
        }
    }
}
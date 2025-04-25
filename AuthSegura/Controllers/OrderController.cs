using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthSegura.Helpers;

namespace AuthSegura.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OrderRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _orderService.CreateOrderAsync(request);
                return Ok(ApiResponse<OrderResponse>.Ok(response, "Orden creada exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var response = await _orderService.GetOrderByIdAsync(id);
                return Ok(ApiResponse<OrderResponse>.Ok(response));
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
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _orderService.GetAllOrdersAsync();
                return Ok(ApiResponse<OrderResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            try
            {
                var response = await _orderService.GetOrdersByUser(userId);
                return Ok(ApiResponse<OrderResponse[]>.Ok(response));
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
        
        [HttpGet("filter")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var response = await _orderService.GetFilterOrderByDate(startDate, endDate);
                return Ok(ApiResponse<OrderResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        [HttpGet("filter/user")]
        public async Task<IActionResult> GetByUserAndDate([FromQuery] int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var response = await _orderService.GetFilterOrderByUserByDate(userId, startDate, endDate);
                return Ok(ApiResponse<OrderResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}

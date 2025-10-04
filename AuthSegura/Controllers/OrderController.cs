using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthSegura.Helpers;
using System.Security.Claims;

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
        public async Task<IActionResult> Create([FromBody] OrderItemRequest[] request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                // Obtener el userId de las claims del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<object>.Fail("No se pudo identificar al usuario"));
                }
            
                
                var response = await _orderService.CreateOrderAsync(request, userId);
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
                
                // Verificar si la orden pertenece al usuario autenticado
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId) && !User.IsInRole("admin") && response.UserId != userId)
                {
                    return Forbid(); // El usuario no puede ver órdenes que no le pertenecen
                }
                
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
        [Authorize(Roles = "admin")]
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
        
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<object>.Fail("No se pudo identificar al usuario"));
                }
                
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
        
        // Este endpoint solo debería ser accesible para administradores
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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
        
        [HttpGet("my-orders/filter")]
        public async Task<IActionResult> GetMyOrdersByDate([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(ApiResponse<object>.Fail("No se pudo identificar al usuario"));
                }
                
                var response = await _orderService.GetFilterOrderByUserByDate(userId, startDate, endDate);
                return Ok(ApiResponse<OrderResponse[]>.Ok(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        // Este endpoint solo debería ser accesible para administradores
        [HttpGet("filter/user")]
        [Authorize(Roles = "admin")]
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

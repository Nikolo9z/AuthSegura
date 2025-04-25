using AuthSegura.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using AuthSegura.Helpers;
using AuthSegura.DTOs;

namespace AuthSegura.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        public UserController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _authService.RegisterAsync(request);
                var accessTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1),
                };
                
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(7),
                };
                
                Response.Cookies.Append("accessToken", response.accessToken, accessTokenOptions);
                Response.Cookies.Append("refreshToken", response.refreshToken, refreshTokenOptions);
                
                return Ok(ApiResponse<AuthResponse>.Ok(response, "Usuario registrado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _authService.LoginAsync(request);
                var accessTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(1),
                };
                
                var refreshTokenOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddHours(7),
                };
                
                Response.Cookies.Append("accessToken", response.accessToken, accessTokenOptions);
                Response.Cookies.Append("refreshToken", response.refreshToken, refreshTokenOptions);
                
                return Ok(ApiResponse<AuthResponse>.Ok(response, "Inicio de sesión exitoso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(ApiResponse<AuthResponse>.Ok(response, "Token renovado exitosamente"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest? refreshToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Datos de entrada inválidos"));
            }
            try
            {
                await _authService.LogoutAsync(refreshToken);
                Response.Cookies.Delete("accessToken");
                Response.Cookies.Delete("refreshToken");
                return Ok(ApiResponse<string>.Ok("Sesión cerrada", "Cierre de sesión exitoso"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}

using AuthSegura.DTOs;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace AuthSegura.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshRequest  request);
        Task <bool> LogoutAsync(LogoutRequest? refreshToken);

    }
}

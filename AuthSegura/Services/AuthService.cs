using AuthSegura.DataAccess;
using AuthSegura.DTOs;
using AuthSegura.Helpers;
using AuthSegura.Models;
using AuthSegura.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AuthSegura.Services
{
    public class AuthService(AppDbContext dbContext, IConfiguration config) : IAuthService
    {
        private readonly AppDbContext _dbContext = dbContext;
        private readonly IConfiguration _config = config;

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _dbContext.Users.Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email) ?? throw new Exception("Invalid credentials");
            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Invalid credentials");
            }
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            };
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();
            var accessToken = JwtHelper.GenerateJwtToken(user, _config["JwtSettings:Secret"]);
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshRequest request)
        {
            if(string.IsNullOrEmpty(request.refreshToken))
            {
                throw new Exception("refresh token is required");
            }
            var refreshToken = await _dbContext.RefreshTokens.Where(rt=> rt.Token == request.refreshToken).FirstOrDefaultAsync() ?? throw new Exception("Invalid Refresh token");
            if (refreshToken.ExpiresAt < DateTime.UtcNow) {
                throw new Exception("Refresh token has expired");
            }
            var user = await _dbContext.Users.FindAsync(refreshToken.UserId);
            if (user?.IsActive != true)
            {
                throw new Exception(" User not found or is inactive");
            }
            var newAccessToken = JwtHelper.GenerateJwtToken(user, _config["JwtSettings:Secret"]);
            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
            };
            _dbContext.RefreshTokens.Remove(refreshToken);
            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync();
            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var userExists = await _dbContext.Users.AnyAsync(u => u.Email == request.Email);
                if (userExists)
                    throw new Exception("User already exists");

                var user = new User
                {
                    Username = request.Name,
                    Email = request.Email,
                    PasswordHash = PasswordHasher.HashPassword(request.Password),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    UserId = user.Id
                };

                _dbContext.RefreshTokens.Add(refreshToken);
                await _dbContext.SaveChangesAsync();

                var accessToken = JwtHelper.GenerateJwtToken(user, _config["JwtSettings:Secret"]);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear usuario" + ex);
            }
        }

        public async Task<bool> LogoutAsync(LogoutRequest? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken.RefreshToken))
            {
                return true;
            }
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken.RefreshToken);
            if (token == null ||token.ExpiresAt < DateTime.UtcNow)
            {
                return true;
            }
            _dbContext.RefreshTokens.Remove(token);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

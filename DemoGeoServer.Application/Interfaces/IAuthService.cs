using DemoGeoServer.Application.DTOs.Auth;

namespace DemoGeoServer.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(int userId);
    }
}
using DemoGeoServer.Application.DTOs.Auth;
using DemoGeoServer.Application.Interfaces;
using DemoGeoServer.Domain.Entities;
using DemoGeoServer.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using BCrypt.Net;

namespace DemoGeoServer.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            ITokenService tokenService,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<JwtSettings> jwtSettings)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Username and password are required"
                    };
                }

                // Get user from database
                var user = await _userRepository.GetByUsernameAsync(request.Username);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                // Generate tokens
                var accessToken = _tokenService.GenerateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

                // Save refresh token to database
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

                return new LoginResponse
                {
                    Success = true,
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // Validate refresh token from database
                var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (tokenEntity == null || tokenEntity.ExpiryDate < DateTime.UtcNow)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                // Get user
                var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateToken(user);
                var newRefreshToken = _tokenService.GenerateRefreshToken();
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

                // Update refresh token in database
                tokenEntity.Token = newRefreshToken;
                tokenEntity.ExpiryDate = DateTime.UtcNow.AddDays(7);
                tokenEntity.UpdatedAt = DateTime.UtcNow;
                await _refreshTokenRepository.UpdateAsync(tokenEntity);

                return new LoginResponse
                {
                    Success = true,
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt,
                    Message = "Token refreshed successfully"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                // Delete all refresh tokens for the user
                await _refreshTokenRepository.DeleteByUserIdAsync(userId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(request.Username) ||
                    string.IsNullOrEmpty(request.Email) ||
                    string.IsNullOrEmpty(request.Password))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "All fields are required"
                    };
                }

                // Check if user already exists
                if (await _userRepository.ExistsAsync(request.Username, request.Email))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Username or email already exists"
                    };
                }

                // Hash password
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Create user
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Role = request.Role ?? "User",
                    CreatedAt = DateTime.UtcNow
                };

                user = await _userRepository.CreateAsync(user);

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Registration successful. Please login to continue.",
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                };
            }
            catch (Exception ex)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}

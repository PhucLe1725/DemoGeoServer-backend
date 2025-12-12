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
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Email and password are required"
                    };
                }

                // Get user from database by email
                var user = await _userRepository.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    };
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid email or password"
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
                var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

                if (tokenEntity == null || tokenEntity.ExpiryDate < DateTime.UtcNow)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                var newAccessToken = _tokenService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

                await _refreshTokenRepository.UpdateTimestampAsync(tokenEntity.Id);

                return new LoginResponse
                {
                    Success = true,
                    Token = newAccessToken,
                    RefreshToken = refreshToken,
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

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            try
            {
                // Validate refresh token
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return false;
                }

                // Get the refresh token entity
                var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                
                if (tokenEntity == null)
                {
                    return false;
                }

                // Delete only this specific refresh token
                await _refreshTokenRepository.DeleteAsync(tokenEntity.Id);
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

                if (await _userRepository.ExistsAsync(request.Username, request.Email))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Username or email already exists"
                    };
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

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

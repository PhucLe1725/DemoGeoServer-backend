using DemoGeoServer.Application.DTOs.Auth;
using DemoGeoServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DemoGeoServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (!response.Success)
                return Unauthorized(response);

            return Ok(response);
        }

        [HttpPost("logout/{userId}")]
        public async Task<ActionResult> Logout(int userId)
        {
            var result = await _authService.LogoutAsync(userId);

            if (!result)
                return BadRequest(new { message = "Logout failed" });

            return Ok(new { message = "Logout successful" });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DemoGeoServer.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok(new { message = "This is a public endpoint - no authentication required" });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            var role = User.FindFirst("role")?.Value
                ?? User.FindFirst(ClaimTypes.Role)?.Value;

            // Debug: Log all claims
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                message = "This is a protected endpoint - authentication required",
                userId,
                role,
                claims = allClaims
            });
        }

        [Authorize]
        [HttpGet("check-role")]
        public IActionResult CheckRole()
        {
            var identity = User.Identity as ClaimsIdentity;
            var roleClaimType = identity?.RoleClaimType;
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            var isAdmin = User.IsInRole("Admin");
            var isUser = User.IsInRole("User");

            var roleClaim = User.FindFirst("role")?.Value;
            var roleClaimStandard = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                message = "Role check endpoint - shows how ASP.NET Core sees your role claims",
                roleClaimType,
                isAdmin,
                isUser,
                roleClaim,
                roleClaimStandard,
                claims = allClaims
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult Admin()
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            var role = User.FindFirst("role")?.Value
                ?? User.FindFirst(ClaimTypes.Role)?.Value;

            // Debug: Log all claims
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            // Check if user is in Admin role
            var isInAdminRole = User.IsInRole("Admin");
            var identity = User.Identity as ClaimsIdentity;
            var roleClaimType = identity?.RoleClaimType;

            return Ok(new
            {
                message = "This is an admin-only endpoint",
                userId,
                role,
                isInAdminRole,
                roleClaimType,
                claims = allClaims
            });
        }
    }
}

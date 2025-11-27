using System.ComponentModel.DataAnnotations;

namespace DemoGeoServer.Application.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}

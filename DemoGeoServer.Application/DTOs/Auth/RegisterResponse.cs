namespace DemoGeoServer.Application.DTOs.Auth
{
 public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }
}

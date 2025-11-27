namespace DemoGeoServer.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Message { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? RefreshToken { get; set; }
    }
}

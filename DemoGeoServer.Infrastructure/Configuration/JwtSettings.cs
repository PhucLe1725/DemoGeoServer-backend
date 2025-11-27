namespace DemoGeoServer.Infrastructure.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "JwtConfig";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; }
    }
}

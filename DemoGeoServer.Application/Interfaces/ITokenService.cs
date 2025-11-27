using DemoGeoServer.Domain.Entities;

namespace DemoGeoServer.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        int? ValidateToken(string token);
        string GenerateRefreshToken();
    }
}

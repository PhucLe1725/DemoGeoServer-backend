using DemoGeoServer.Domain.Entities;

namespace DemoGeoServer.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
        Task<bool> UpdateTimestampAsync(int tokenId);
    }
}

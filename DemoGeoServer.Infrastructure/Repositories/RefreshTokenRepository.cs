using DemoGeoServer.Application.Interfaces;
using DemoGeoServer.Domain.Entities;
using DemoGeoServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DemoGeoServer.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
        {
            refreshToken.CreatedAt = DateTime.UtcNow;
            refreshToken.UpdatedAt = DateTime.UtcNow;
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var token = await _context.RefreshTokens.FindAsync(id);
            if (token == null)
                return false;

            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                            .Where(rt => rt.UserId == userId)
                            .ToListAsync();

            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTimestampAsync(int tokenId)
        {
            var token = await _context.RefreshTokens.FindAsync(tokenId);
            if (token == null)
                return false;

            token.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
        {
            refreshToken.UpdatedAt = DateTime.UtcNow;

            // Check if entity is already being tracked
            var trackedEntity = _context.RefreshTokens.Local
                .FirstOrDefault(e => e.Id == refreshToken.Id);

            if (trackedEntity != null)
            {
                // Entity is already tracked, just update the timestamp
                _context.Entry(trackedEntity).CurrentValues.SetValues(refreshToken);
            }
            else
            {
                // Entity is not tracked, attach and mark as modified
                _context.RefreshTokens.Update(refreshToken);
            }

            await _context.SaveChangesAsync();
            return refreshToken;
        }
    }
}

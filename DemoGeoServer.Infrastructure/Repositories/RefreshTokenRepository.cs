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
                .Include(rt => rt.User)
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

        public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
        {
            refreshToken.UpdatedAt = DateTime.UtcNow;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }
    }
}

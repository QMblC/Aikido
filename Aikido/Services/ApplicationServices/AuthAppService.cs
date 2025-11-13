using Aikido.Data;
using Aikido.Dto.Auth;
using Aikido.Entities;
using Aikido.Services;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Application.Services
{
    public class AuthApplicationService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthApplicationService(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<(UserEntity user, string accessToken, RefreshTokenEntity refreshToken)?>
            AuthenticateAsync(string login, string password, string? deviceInfo = null)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || string.IsNullOrEmpty(user.Password))
                return null;

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isPasswordValid)
                return null;

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user.Id, deviceInfo);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return (user, accessToken, refreshToken);
        }

        public async Task<(string accessToken, RefreshTokenEntity refreshToken)?>
            RefreshTokenAsync(string refreshTokenValue)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

            if (refreshToken == null || !refreshToken.IsActive)
                return null;

            if (refreshToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(refreshToken.UserId);
                return null;
            }

            var newAccessToken = _jwtService.GenerateAccessToken(refreshToken.User);
            var newRefreshToken = _jwtService.GenerateRefreshToken(
                refreshToken.UserId,
                refreshToken.DeviceInfo);

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return (newAccessToken, newRefreshToken);
        }

        public async Task<bool> RevokeTokenAsync(string refreshTokenValue)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue);

            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task RevokeAllUserTokensAsync(long userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow ||
                            (rt.IsRevoked && rt.RevokedAt < DateTime.UtcNow.AddDays(-7)))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}

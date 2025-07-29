using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 用户OAuth绑定仓储实现
/// </summary>
public class UserOAuthBindingRepository : BaseRepository<UserOAuthBinding, Guid>, IUserOAuthBindingRepository
{
    public UserOAuthBindingRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<UserOAuthBinding?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthBinding>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId, cancellationToken);
    }

    public async Task<UserOAuthBinding?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthBinding>()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider, cancellationToken);
    }

    public async Task<IEnumerable<UserOAuthBinding>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthBinding>()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Provider)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsProviderUserIdBoundAsync(string provider, string providerUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthBinding>()
            .AnyAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId, cancellationToken);
    }

    public async Task<bool> IsUserBoundToProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserOAuthBinding>()
            .AnyAsync(x => x.UserId == userId && x.Provider == provider, cancellationToken);
    }

    public async Task UpdateTokenAsync(Guid id, string? accessToken, string? refreshToken, DateTime? tokenExpiredAt, CancellationToken cancellationToken = default)
    {
        var binding = await _context.Set<UserOAuthBinding>().FindAsync(new object[] { id }, cancellationToken);
        if (binding != null)
        {
            binding.AccessToken = accessToken;
            binding.RefreshToken = refreshToken;
            binding.TokenExpiredAt = tokenExpiredAt;
            binding.LastSyncAt = DateTime.UtcNow;
            binding.Status = tokenExpiredAt.HasValue && tokenExpiredAt < DateTime.UtcNow 
                ? OAuthBindingStatus.TokenExpired 
                : OAuthBindingStatus.Active;
        }
    }

    public async Task UpdateStatusAsync(Guid id, OAuthBindingStatus status, CancellationToken cancellationToken = default)
    {
        var binding = await _context.Set<UserOAuthBinding>().FindAsync(new object[] { id }, cancellationToken);
        if (binding != null)
        {
            binding.Status = status;
        }
    }
} 
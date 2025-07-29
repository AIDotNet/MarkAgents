using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// OAuth配置仓储实现
/// </summary>
public class OAuthConfigRepository : BaseRepository<OAuthConfig,Guid>, IOAuthConfigRepository
{
    public OAuthConfigRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<OAuthConfig?> GetByProviderAsync(string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OAuthConfig>()
            .FirstOrDefaultAsync(x => x.Provider == provider, cancellationToken);
    }

    public async Task<IEnumerable<OAuthConfig>> GetEnabledConfigsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<OAuthConfig>()
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Sort)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsProviderExistsAsync(string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Set<OAuthConfig>()
            .AnyAsync(x => x.Provider == provider, cancellationToken);
    }

    public async Task UpdateEnabledStatusAsync(Guid id, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var config = await _context.Set<OAuthConfig>().FindAsync(new object[] { id }, cancellationToken);
        if (config != null)
        {
            config.IsEnabled = isEnabled;
        }
    }
} 
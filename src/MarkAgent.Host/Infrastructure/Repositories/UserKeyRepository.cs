using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 用户密钥仓储实现类
/// </summary>
public class UserKeyRepository : BaseRepository<UserKey, Guid>, IUserKeyRepository
{
    public UserKeyRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<UserKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uk => uk.User)
            .Include(uk => uk.McpToolConfigs)
            .FirstOrDefaultAsync(uk => uk.Key == key && uk.Status == KeyStatus.Active, cancellationToken);
    }

    public async Task<List<UserKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uk => uk.McpToolConfigs)
            .Where(uk => uk.UserId == userId)
            .OrderByDescending(uk => uk.IsDefault)
            .ThenBy(uk => uk.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserKey?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(uk => uk.McpToolConfigs)
            .FirstOrDefaultAsync(uk => uk.UserId == userId && uk.IsDefault && uk.Status == KeyStatus.Active, cancellationToken);
    }

    public async Task<bool> IsKeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(uk => uk.Key == key, cancellationToken);
    }

    public async Task<bool> IsKeyNameExistsForUserAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(uk => uk.UserId == userId && uk.Name == name, cancellationToken);
    }

    public async Task UpdateUsageAsync(string key, CancellationToken cancellationToken = default)
    {
        var userKey = await _dbSet.FirstOrDefaultAsync(uk => uk.Key == key, cancellationToken);
        if (userKey != null)
        {
            userKey.UsageCount++;
            userKey.LastUsedAt = DateTime.UtcNow;
            await UpdateAsync(userKey, cancellationToken);
        }
    }

    public async Task SetDefaultKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default)
    {
        // 先取消所有默认标记
        var userKeys = await _dbSet.Where(uk => uk.UserId == userId).ToListAsync(cancellationToken);
        foreach (var key in userKeys)
        {
            key.IsDefault = key.Id == keyId;
        }
        await UpdateRangeAsync(userKeys, cancellationToken);
    }

    public async Task<int> GetActiveKeysCountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(uk => uk.UserId == userId && uk.Status == KeyStatus.Active, cancellationToken);
    }

    public async Task<(int TotalKeys, int ActiveKeys, long TotalUsage)> GetKeyStatsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var keys = await _dbSet.Where(uk => uk.UserId == userId).ToListAsync(cancellationToken);
        return (
            keys.Count,
            keys.Count(k => k.Status == KeyStatus.Active),
            keys.Sum(k => k.UsageCount)
        );
    }
} 
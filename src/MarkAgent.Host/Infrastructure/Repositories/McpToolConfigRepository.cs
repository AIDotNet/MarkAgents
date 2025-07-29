using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// MCP工具配置仓储实现类
/// </summary>
public class McpToolConfigRepository : BaseRepository<McpToolConfig,Guid>, IMcpToolConfigRepository
{
    public McpToolConfigRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<List<McpToolConfig>> GetByUserKeyIdAsync(Guid userKeyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserKeyId == userKeyId)
            .OrderBy(c => c.ToolName)
            .ToListAsync(cancellationToken);
    }

    public async Task<McpToolConfig?> GetByUserKeyAndToolAsync(Guid userKeyId, string toolName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            c => c.UserKeyId == userKeyId && c.ToolName == toolName, 
            cancellationToken);
    }

    public async Task<List<McpToolConfig>> GetEnabledByUserKeyAsync(Guid userKeyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserKeyId == userKeyId && c.IsEnabled)
            .OrderBy(c => c.ToolName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<McpToolConfig>> GetByToolNameAsync(string toolName, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.UserKey)
            .Where(c => c.ToolName == toolName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsToolConfiguredAsync(Guid userKeyId, string toolName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            c => c.UserKeyId == userKeyId && c.ToolName == toolName, 
            cancellationToken);
    }

    public async Task ToggleToolAsync(Guid userKeyId, string toolName, bool isEnabled, CancellationToken cancellationToken = default)
    {
        var config = await GetByUserKeyAndToolAsync(userKeyId, toolName, cancellationToken);
        if (config != null)
        {
            config.IsEnabled = isEnabled;
            await UpdateAsync(config, cancellationToken);
        }
    }

    public async Task UpdateToolConfigsAsync(Guid userKeyId, List<McpToolConfig> configs, CancellationToken cancellationToken = default)
    {
        // 获取现有配置
        var existingConfigs = await GetByUserKeyIdAsync(userKeyId, cancellationToken);
        
        // 删除不再需要的配置
        var configsToDelete = existingConfigs.Where(ec => !configs.Any(c => c.ToolName == ec.ToolName)).ToList();
        await DeleteRangeAsync(configsToDelete, cancellationToken);
        
        // 更新或添加配置
        foreach (var config in configs)
        {
            var existing = existingConfigs.FirstOrDefault(ec => ec.ToolName == config.ToolName);
            if (existing != null)
            {
                existing.IsEnabled = config.IsEnabled;
                existing.Configuration = config.Configuration;
                existing.Permissions = config.Permissions;
                existing.Version = config.Version;
                existing.DisplayName = config.DisplayName;
                existing.Description = config.Description;
                await UpdateAsync(existing, cancellationToken);
            }
            else
            {
                config.UserKeyId = userKeyId;
                await AddAsync(config, cancellationToken);
            }
        }
    }

    public async Task UpdateUsageStatsAsync(Guid configId, CancellationToken cancellationToken = default)
    {
        var config = await GetByIdAsync(configId, cancellationToken);
        if (config != null)
        {
            config.UsageCount++;
            config.LastUsedAt = DateTime.UtcNow;
            await UpdateAsync(config, cancellationToken);
        }
    }

    public async Task<(int TotalTools, int EnabledTools, long TotalUsage)> GetToolStatsByUserKeyAsync(Guid userKeyId, CancellationToken cancellationToken = default)
    {
        var configs = await GetByUserKeyIdAsync(userKeyId, cancellationToken);
        return (
            configs.Count,
            configs.Count(c => c.IsEnabled),
            configs.Sum(c => c.UsageCount)
        );
    }
} 
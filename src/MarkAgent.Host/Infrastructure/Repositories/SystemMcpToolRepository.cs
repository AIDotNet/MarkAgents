using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 系统MCP工具存储库实现类
/// </summary>
public class SystemMcpToolRepository : BaseRepository<SystemMcpTool, Guid>, ISystemMcpToolRepository
{
    public SystemMcpToolRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<List<SystemMcpTool>> GetEnabledToolsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsEnabled)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SystemMcpTool>> GetToolsByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsEnabled && t.Category == category)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemMcpTool?> GetByToolNameAsync(string toolName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.ToolName == toolName, cancellationToken);
    }

    public async Task<List<SystemMcpTool>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsEnabled && t.Tags != null && t.Tags.Any(tag => tags.Contains(tag)))
            .OrderByDescending(t => t.TotalUsageCount)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SystemMcpTool>> SearchToolsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var loweredSearchTerm = searchTerm.ToLower();
        return await _dbSet
            .Where(t => t.IsEnabled && 
                       (t.DisplayName.ToLower().Contains(loweredSearchTerm) ||
                        t.ToolName.ToLower().Contains(loweredSearchTerm) ||
                        (t.Description != null && t.Description.ToLower().Contains(loweredSearchTerm))))
            .OrderByDescending(t => t.TotalUsageCount)
            .ThenBy(t => t.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int TotalTools, int EnabledTools, int BuiltInTools)> GetToolStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var totalTools = await _dbSet.CountAsync(cancellationToken);
        var enabledTools = await _dbSet.CountAsync(t => t.IsEnabled, cancellationToken);
        var builtInTools = await _dbSet.CountAsync(t => t.IsBuiltIn, cancellationToken);
        
        return (totalTools, enabledTools, builtInTools);
    }

    public async Task UpdateUsageCountAsync(Guid toolId, CancellationToken cancellationToken = default)
    {
        var tool = await _dbSet.FindAsync(new object[] { toolId }, cancellationToken);
        if (tool != null)
        {
            tool.TotalUsageCount++;
            tool.LastUpdateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<SystemMcpTool>> GetPopularToolsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsEnabled)
            .OrderByDescending(t => t.TotalUsageCount)
            .ThenBy(t => t.DisplayName)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ToolNameExistsAsync(string toolName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.ToolName == toolName);
        
        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }
} 
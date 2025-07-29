using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// MCP工具使用记录仓储实现类
/// </summary>
public class McpToolUsageRepository : BaseRepository<McpToolUsage,Guid>, IMcpToolUsageRepository
{
    public McpToolUsageRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<List<McpToolUsage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserKey)
            .Include(u => u.Conversation)
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<McpToolUsage>> GetByUserKeyIdAsync(Guid userKeyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.UserKey)
            .Include(u => u.Conversation)
            .Where(u => u.UserKeyId == userKeyId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<McpToolUsage>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.ConversationId == conversationId)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<McpToolUsage>> GetByToolNameAsync(string toolName, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.ToolName == toolName);
        
        if (userId.HasValue)
        {
            query = query.Where(u => u.UserId == userId.Value);
        }
        
        return await query
            .Include(u => u.UserKey)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<McpToolUsage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
        
        if (userId.HasValue)
        {
            query = query.Where(u => u.UserId == userId.Value);
        }
        
        return await query
            .Include(u => u.UserKey)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetToolUsageStatsByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.UserId == userId);
        
        if (startDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= endDate.Value);
        }
        
        return await query
            .GroupBy(u => u.ToolName)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetToolPopularityStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= endDate.Value);
        }
        
        return await query
            .GroupBy(u => u.ToolName)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    public async Task<Dictionary<string, double>> GetAverageExecutionTimeByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.UserId == userId && u.ExecutionTimeMs > 0)
            .GroupBy(u => u.ToolName)
            .ToDictionaryAsync(g => g.Key, g => g.Average(x => x.ExecutionTimeMs), cancellationToken);
    }

    public async Task<Dictionary<string, double>> GetToolErrorRatesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= endDate.Value);
        }
        
        var toolStats = await query
            .GroupBy(u => u.ToolName)
            .Select(g => new 
            {
                ToolName = g.Key,
                TotalCalls = g.Count(),
                ErrorCalls = g.Count(x => x.Status != ExecutionStatus.Success)
            })
            .ToListAsync(cancellationToken);
        
        return toolStats.ToDictionary(
            s => s.ToolName, 
            s => s.TotalCalls > 0 ? (double)s.ErrorCalls / s.TotalCalls * 100 : 0);
    }

    public async Task<McpToolUsage> RecordUsageAsync(McpToolUsage usage, CancellationToken cancellationToken = default)
    {
        return await AddAsync(usage, cancellationToken);
    }

    public async Task<long> GetTokenUsageByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(u => u.UserId == userId);
        
        if (startDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= endDate.Value);
        }
        
        return await query.SumAsync(u => u.TokensUsed, cancellationToken);
    }

    public async Task<long> GetTotalToolCallsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= endDate.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }
} 
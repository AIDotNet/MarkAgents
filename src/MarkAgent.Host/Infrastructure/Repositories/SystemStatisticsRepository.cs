using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 系统统计仓储实现类
/// </summary>
public class SystemStatisticsRepository : BaseRepository<SystemStatistics,Guid>, ISystemStatisticsRepository
{
    public SystemStatisticsRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<SystemStatistics?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        return await _dbSet.FirstOrDefaultAsync(s => s.StatDate.Date == dateOnly, cancellationToken);
    }

    public async Task<List<SystemStatistics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.StatDate >= startDate && s.StatDate <= endDate)
            .OrderBy(s => s.StatDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<SystemStatistics> CreateOrUpdateAsync(SystemStatistics statistics, CancellationToken cancellationToken = default)
    {
        var existing = await GetByDateAsync(statistics.StatDate, cancellationToken);

        if (existing != null)
        {
            // 更新现有统计
            existing.TotalUsers = statistics.TotalUsers;
            existing.ActiveUsers = statistics.ActiveUsers;
            existing.NewUsers = statistics.NewUsers;
            existing.TotalUserKeys = statistics.TotalUserKeys;
            existing.TotalTodos = statistics.TotalTodos;
            existing.CompletedTodos = statistics.CompletedTodos;
            existing.TotalConversations = statistics.TotalConversations;
            existing.TotalTokensUsed = statistics.TotalTokensUsed;
            existing.TotalApiCalls = statistics.TotalApiCalls;

            return await UpdateAsync(existing, cancellationToken);
        }
        else
        {
            return await AddAsync(statistics, cancellationToken);
        }
    }

    public async Task RefreshSystemStatisticsAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dateOnly = date.Date;
        var startOfDay = dateOnly;
        var endOfDay = dateOnly.AddDays(1).AddTicks(-1);

        // 统计总用户数
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        
        // 统计活跃用户数
        var activeUsers = await _context.Users.CountAsync(u => u.Status == UserStatus.Active, cancellationToken);
        
        // 统计新用户数
        var newUsers = await _context.Users.CountAsync(
            u => u.CreatedAt >= startOfDay && u.CreatedAt <= endOfDay, 
            cancellationToken);
        
        // 统计总密钥数
        var totalUserKeys = await _context.UserKeys.CountAsync(cancellationToken);
        
        // 统计总Todo数
        var totalTodos = await _context.Todos.CountAsync(cancellationToken);
        
        // 统计完成的Todo数
        var completedTodos = await _context.Todos.CountAsync(t => t.Status == TodoStatus.Completed, cancellationToken);
        
        // 统计总对话数
        var totalConversations = await _context.Conversations.CountAsync(cancellationToken);
        
        
        // 统计总API调用次数（基于工具使用记录）
        var totalApiCalls = await _context.McpToolUsages.CountAsync(cancellationToken);

        var systemStats = new SystemStatistics
        {
            StatDate = dateOnly,
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            NewUsers = newUsers,
            TotalUserKeys = totalUserKeys,
            TotalTodos = totalTodos,
            CompletedTodos = completedTodos,
            TotalConversations = totalConversations,
            TotalApiCalls = totalApiCalls
        };

        await CreateOrUpdateAsync(systemStats, cancellationToken);
    }
} 
using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 每日工具统计仓储实现
/// </summary>
public class DailyToolStatisticsRepository : IDailyToolStatisticsRepository
{
    private readonly StatisticsDbContext _context;

    public DailyToolStatisticsRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<DailyToolStatistics> AddOrUpdateAsync(DailyToolStatistics statistics)
    {
        var existing = await _context.DailyToolStatistics
            .FirstOrDefaultAsync(s => s.Date == statistics.Date && s.ToolName == statistics.ToolName);

        if (existing != null)
        {
            // 更新现有记录
            existing.TotalUsageCount = statistics.TotalUsageCount;
            existing.SuccessCount = statistics.SuccessCount;
            existing.FailureCount = statistics.FailureCount;
            existing.AverageExecutionTimeMs = statistics.AverageExecutionTimeMs;
            existing.MinExecutionTimeMs = statistics.MinExecutionTimeMs;
            existing.MaxExecutionTimeMs = statistics.MaxExecutionTimeMs;
            existing.TotalInputSize = statistics.TotalInputSize;
            existing.TotalOutputSize = statistics.TotalOutputSize;
            existing.UniqueSessionCount = statistics.UniqueSessionCount;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.DailyToolStatistics.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // 新增记录
            _context.DailyToolStatistics.Add(statistics);
            await _context.SaveChangesAsync();
            return statistics;
        }
    }

    public async Task<List<DailyToolStatistics>> GetStatisticsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.DailyToolStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<DailyToolStatistics>> GetStatisticsByToolNameAsync(string toolName, DateOnly? startDate = null, int days = 30)
    {
        var query = _context.DailyToolStatistics
            .Where(s => s.ToolName == toolName);

        var endDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeStartDate = endDate.AddDays(-days);

        query = query.Where(s => s.Date >= rangeStartDate && s.Date <= endDate);

        return await query
            .OrderBy(s => s.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<string, (int totalUsage, double successRate)>> GetToolSummaryAsync(int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        return await _context.DailyToolStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => s.ToolName)
            .Select(g => new
            {
                ToolName = g.Key,
                TotalUsage = g.Sum(s => s.TotalUsageCount),
                SuccessRate = g.Sum(s => s.TotalUsageCount) > 0 
                    ? (double)g.Sum(s => s.SuccessCount) / g.Sum(s => s.TotalUsageCount) * 100 
                    : 0
            })
            .ToDictionaryAsync(x => x.ToolName, x => (x.TotalUsage, x.SuccessRate));
    }

    public async Task GenerateDailyStatisticsAsync(DateOnly date)
    {
        var startDate = date.ToDateTime(TimeOnly.MinValue);
        var endDate = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // 从原始使用记录中计算每日统计
        var dailyStats = await _context.ToolUsageRecords
            .Where(r => r.StartTime >= startDate && r.StartTime < endDate)
            .GroupBy(r => r.ToolName)
            .Select(g => new DailyToolStatistics
            {
                Date = date,
                ToolName = g.Key,
                TotalUsageCount = g.Count(),
                SuccessCount = g.Count(r => r.IsSuccess),
                FailureCount = g.Count(r => !r.IsSuccess),
                AverageExecutionTimeMs = g.Average(r => r.DurationMs),
                MinExecutionTimeMs = g.Min(r => r.DurationMs),
                MaxExecutionTimeMs = g.Max(r => r.DurationMs),
                TotalInputSize = g.Sum(r => (long)r.InputSize),
                TotalOutputSize = g.Sum(r => (long)r.OutputSize),
                UniqueSessionCount = g.Where(r => !string.IsNullOrEmpty(r.SessionId))
                                   .Select(r => r.SessionId)
                                   .Distinct()
                                   .Count()
            })
            .AsNoTracking()
            .ToListAsync();

        // 保存或更新统计数据
        foreach (var stats in dailyStats)
        {
            await AddOrUpdateAsync(stats);
        }
    }

    public async Task<List<(string toolName, int usageCount)>> GetMostActiveToolsAsync(int topCount = 10, int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        return await _context.DailyToolStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => s.ToolName)
            .Select(g => new { ToolName = g.Key, UsageCount = g.Sum(s => s.TotalUsageCount) })
            .OrderByDescending(x => x.UsageCount)
            .Take(topCount)
            .Select(x => ValueTuple.Create(x.ToolName, x.UsageCount))
            .ToListAsync();
    }
}
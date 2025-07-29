using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// 每日客户端统计仓储实现
/// </summary>
public class DailyClientStatisticsRepository : IDailyClientStatisticsRepository
{
    private readonly StatisticsDbContext _context;

    public DailyClientStatisticsRepository(StatisticsDbContext context)
    {
        _context = context;
    }

    public async Task<DailyClientStatistics> AddOrUpdateAsync(DailyClientStatistics statistics)
    {
        var existing = await _context.DailyClientStatistics
            .FirstOrDefaultAsync(s => s.Date == statistics.Date && 
                                    s.ClientName == statistics.ClientName && 
                                    s.ClientVersion == statistics.ClientVersion);

        if (existing != null)
        {
            // 更新现有记录
            existing.TotalConnections = statistics.TotalConnections;
            existing.SuccessfulConnections = statistics.SuccessfulConnections;
            existing.FailedConnections = statistics.FailedConnections;
            existing.TotalConnectionDurationSeconds = statistics.TotalConnectionDurationSeconds;
            existing.AverageConnectionDurationSeconds = statistics.AverageConnectionDurationSeconds;
            existing.MaxConnectionDurationSeconds = statistics.MaxConnectionDurationSeconds;
            existing.MinConnectionDurationSeconds = statistics.MinConnectionDurationSeconds;
            existing.TotalToolUsages = statistics.TotalToolUsages;
            existing.AverageToolUsagesPerConnection = statistics.AverageToolUsagesPerConnection;
            existing.UniqueSessionCount = statistics.UniqueSessionCount;
            existing.FirstConnectionTime = statistics.FirstConnectionTime;
            existing.LastConnectionTime = statistics.LastConnectionTime;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.DailyClientStatistics.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }
        else
        {
            // 新增记录
            _context.DailyClientStatistics.Add(statistics);
            await _context.SaveChangesAsync();
            return statistics;
        }
    }

    public async Task<List<DailyClientStatistics>> GetStatisticsByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.DailyClientStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderBy(s => s.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<DailyClientStatistics>> GetStatisticsByClientNameAsync(string clientName, DateOnly? startDate = null, int days = 30)
    {
        var query = _context.DailyClientStatistics
            .Where(s => s.ClientName == clientName);

        var endDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rangeStartDate = endDate.AddDays(-days);

        query = query.Where(s => s.Date >= rangeStartDate && s.Date <= endDate);

        return await query
            .OrderBy(s => s.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<string, (int totalConnections, double successRate, double avgDuration)>> GetClientSummaryAsync(int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        return await _context.DailyClientStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => s.ClientName)
            .Select(g => new
            {
                ClientName = g.Key,
                TotalConnections = g.Sum(s => s.TotalConnections),
                SuccessRate = g.Sum(s => s.TotalConnections) > 0 
                    ? (double)g.Sum(s => s.SuccessfulConnections) / g.Sum(s => s.TotalConnections) * 100 
                    : 0,
                AvgDuration = g.Average(s => s.AverageConnectionDurationSeconds)
            })
            .ToDictionaryAsync(x => x.ClientName, x => (x.TotalConnections, x.SuccessRate, x.AvgDuration));
    }

    public async Task GenerateDailyClientStatisticsAsync(DateOnly date)
    {
        var startDate = date.ToDateTime(TimeOnly.MinValue);
        var endDate = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // 从原始连接记录中计算每日客户端统计
        var dailyStats = await _context.ClientConnectionRecords
            .Where(r => r.ConnectionTime >= startDate && r.ConnectionTime < endDate)
            .GroupBy(r => new { r.ClientName, r.ClientVersion })
            .Select(g => new DailyClientStatistics
            {
                Date = date,
                ClientName = g.Key.ClientName,
                ClientVersion = g.Key.ClientVersion,
                TotalConnections = g.Count(),
                SuccessfulConnections = g.Count(r => r.Status == ClientConnectionStatus.Connected || 
                                                  r.Status == ClientConnectionStatus.Disconnected),
                FailedConnections = g.Count(r => r.Status == ClientConnectionStatus.Failed || 
                                               r.Status == ClientConnectionStatus.Timeout),
                TotalConnectionDurationSeconds = g.Where(r => r.ConnectionDurationSeconds.HasValue)
                                                 .Sum(r => r.ConnectionDurationSeconds!.Value),
                AverageConnectionDurationSeconds = g.Where(r => r.ConnectionDurationSeconds.HasValue)
                                                   .Average(r => (double?)r.ConnectionDurationSeconds) ?? 0,
                MaxConnectionDurationSeconds = g.Where(r => r.ConnectionDurationSeconds.HasValue)
                                              .Max(r => (long?)r.ConnectionDurationSeconds) ?? 0,
                MinConnectionDurationSeconds = g.Where(r => r.ConnectionDurationSeconds.HasValue)
                                              .Min(r => (long?)r.ConnectionDurationSeconds) ?? 0,
                TotalToolUsages = g.Sum(r => r.ToolUsageCount),
                AverageToolUsagesPerConnection = g.Count() > 0 ? (double)g.Sum(r => r.ToolUsageCount) / g.Count() : 0,
                UniqueSessionCount = g.Where(r => !string.IsNullOrEmpty(r.SessionId))
                                   .Select(r => r.SessionId)
                                   .Distinct()
                                   .Count(),
                FirstConnectionTime = g.Min(r => r.ConnectionTime),
                LastConnectionTime = g.Max(r => r.ConnectionTime)
            })
            .AsNoTracking()
            .ToListAsync();

        // 保存或更新统计数据
        foreach (var stats in dailyStats)
        {
            await AddOrUpdateAsync(stats);
        }
    }

    public async Task<List<(string clientName, int connectionCount, double successRate)>> GetMostActiveClientsAsync(int topCount = 10, int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        return await _context.DailyClientStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .GroupBy(s => s.ClientName)
            .Select(g => new { 
                ClientName = g.Key, 
                ConnectionCount = g.Sum(s => s.TotalConnections),
                SuccessRate = g.Sum(s => s.TotalConnections) > 0 
                    ? (double)g.Sum(s => s.SuccessfulConnections) / g.Sum(s => s.TotalConnections) * 100 
                    : 0
            })
            .OrderByDescending(x => x.ConnectionCount)
            .Take(topCount)
            .Select(x => ValueTuple.Create(x.ClientName, x.ConnectionCount, x.SuccessRate))
            .ToListAsync();
    }

    public async Task<List<(DateOnly date, Dictionary<string, int> clientConnections)>> GetClientConnectionTrendAsync(int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        var rawData = await _context.DailyClientStatistics
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .Select(s => new { s.Date, s.ClientName, s.TotalConnections })
            .AsNoTracking()
            .ToListAsync();

        return rawData
            .GroupBy(x => x.Date)
            .Select(g => (
                date: g.Key,
                clientConnections: g.ToDictionary(x => x.ClientName, x => x.TotalConnections)
            ))
            .OrderBy(x => x.date)
            .ToList();
    }
}
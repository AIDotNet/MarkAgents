using System.Text;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Domain.Services;

namespace MarkAgent.Host.Infrastructure.Services;

/// <summary>
/// 工具统计服务实现
/// </summary>
public class ToolStatisticsService : IToolStatisticsService
{
    private readonly IToolUsageRepository _toolUsageRepository;
    private readonly IDailyToolStatisticsRepository _dailyStatsRepository;
    private readonly IClientConnectionRepository _clientConnectionRepository;
    private readonly IDailyClientStatisticsRepository _dailyClientStatsRepository;

    public ToolStatisticsService(
        IToolUsageRepository toolUsageRepository,
        IDailyToolStatisticsRepository dailyStatsRepository,
        IClientConnectionRepository clientConnectionRepository,
        IDailyClientStatisticsRepository dailyClientStatsRepository)
    {
        _toolUsageRepository = toolUsageRepository;
        _dailyStatsRepository = dailyStatsRepository;
        _clientConnectionRepository = clientConnectionRepository;
        _dailyClientStatsRepository = dailyClientStatsRepository;
    }

    public async Task RecordToolUsageAsync(string toolName, DateTime startTime, DateTime endTime, 
        bool isSuccess, string? errorMessage = null, int inputSize = 0, int outputSize = 0,
        string? parametersJson = null, string? sessionId = null, 
        string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var record = new ToolUsageRecord
            {
                ToolName = toolName,
                SessionId = sessionId,
                StartTime = startTime,
                EndTime = endTime,
                DurationMs = (long)(endTime - startTime).TotalMilliseconds,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                InputSize = inputSize,
                OutputSize = outputSize,
                ParametersJson = parametersJson,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow
            };

            await _toolUsageRepository.AddAsync(record);
            Console.WriteLine($"✅ Recorded tool usage: {toolName} (Session: {sessionId}) - Success: {isSuccess}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to record tool usage: {ex.Message}");
        }

        // 异步生成或更新当日统计（避免阻塞主流程）
        _ = Task.Run(async () =>
        {
            try
            {
                var date = DateOnly.FromDateTime(startTime);
                await _dailyStatsRepository.GenerateDailyStatisticsAsync(date);
                
                // 如果有会话ID，增加该客户端连接的工具使用计数
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await IncrementClientToolUsageAsync(sessionId);
                }
            }
            catch
            {
                // 记录日志但不影响主流程
            }
        });
    }

    public async Task<OverviewStatistics> GetOverviewStatisticsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfToday = today.ToDateTime(TimeOnly.MinValue);
        var endOfToday = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // 获取总使用次数
        var totalUsage = await _toolUsageRepository.GetTotalUsageCountAsync();

        // 获取今日使用记录
        var todayRecords = await _toolUsageRepository.GetRecordsByDateRangeAsync(startOfToday, endOfToday);

        // 获取活跃工具数量（最近30天有使用的工具）
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentUsageStats = await _toolUsageRepository.GetUsageStatsByToolAsync(thirtyDaysAgo);
        var activeToolsCount = recentUsageStats.Count;

        // 获取最近7天的记录计算平均成功率和执行时间
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentRecords = await _toolUsageRepository.GetRecordsByDateRangeAsync(sevenDaysAgo, DateTime.UtcNow);

        var averageSuccessRate = recentRecords.Count > 0 
            ? (double)recentRecords.Count(r => r.IsSuccess) / recentRecords.Count * 100 
            : 0;

        var averageExecutionTime = recentRecords.Count > 0 
            ? recentRecords.Average(r => r.DurationMs) 
            : 0;

        // 获取今日成功率
        var todaySuccessRate = todayRecords.Count > 0 
            ? (double)todayRecords.Count(r => r.IsSuccess) / todayRecords.Count * 100 
            : 0;

        // 获取独立会话数（今日）
        var uniqueSessions = todayRecords
            .Where(r => !string.IsNullOrEmpty(r.SessionId))
            .Select(r => r.SessionId)
            .Distinct()
            .Count();

        // 获取最常用工具
        var mostUsedTool = recentUsageStats.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "无";

        return new OverviewStatistics
        {
            TotalUsageCount = totalUsage,
            ActiveToolsCount = activeToolsCount,
            AverageSuccessRate = averageSuccessRate,
            AverageExecutionTime = averageExecutionTime,
            TodayUsageCount = todayRecords.Count,
            TodaySuccessRate = todaySuccessRate,
            UniqueSessions = uniqueSessions,
            MostUsedTool = mostUsedTool
        };
    }

    public async Task<List<TrendDataPoint>> GetUsageTrendDataAsync(int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        var dailyStats = await _dailyStatsRepository.GetStatisticsByDateRangeAsync(startDate, endDate);

        // 按日期分组汇总
        var trendData = dailyStats
            .GroupBy(s => s.Date)
            .Select(g => new TrendDataPoint
            {
                Date = g.Key.ToDateTime(TimeOnly.MinValue),
                TotalUsage = g.Sum(s => s.TotalUsageCount),
                SuccessCount = g.Sum(s => s.SuccessCount),
                FailureCount = g.Sum(s => s.FailureCount),
                SuccessRate = g.Sum(s => s.TotalUsageCount) > 0 
                    ? (double)g.Sum(s => s.SuccessCount) / g.Sum(s => s.TotalUsageCount) * 100 
                    : 0,
                AverageExecutionTime = g.Average(s => s.AverageExecutionTimeMs)
            })
            .OrderBy(t => t.Date)
            .ToList();

        return trendData;
    }

    public async Task<List<ToolUsageDistribution>> GetToolUsageDistributionAsync(int days = 30)
    {
        // 直接从原始记录计算分布，避免依赖每日统计表
        var startDate = DateTime.UtcNow.AddDays(-days);
        var endDate = DateTime.UtcNow;

        var usageStats = await _toolUsageRepository.GetUsageStatsByToolAsync(startDate);
        var totalUsage = usageStats.Values.Sum();

        if (totalUsage == 0) return new List<ToolUsageDistribution>();

        var colors = new[] { "#8884d8", "#82ca9d", "#ffc658", "#ff7c7c", "#8dd1e1", "#d084d0", "#87ceeb", "#ffa07a" };

        return usageStats.Select((kvp, index) => new ToolUsageDistribution
        {
            ToolName = kvp.Key,
            UsageCount = kvp.Value,
            Percentage = (double)kvp.Value / totalUsage * 100,
            Color = colors[index % colors.Length]
        })
        .OrderByDescending(d => d.UsageCount)
        .ToList();
    }

    public async Task<List<ToolSuccessRateStats>> GetToolSuccessRateStatsAsync(int days = 30)
    {
        var summary = await _dailyStatsRepository.GetToolSummaryAsync(days);

        return summary.Select(kvp => new ToolSuccessRateStats
        {
            ToolName = kvp.Key,
            TotalUsageCount = kvp.Value.totalUsage,
            SuccessCount = (int)(kvp.Value.totalUsage * kvp.Value.successRate / 100),
            FailureCount = kvp.Value.totalUsage - (int)(kvp.Value.totalUsage * kvp.Value.successRate / 100),
            SuccessRate = kvp.Value.successRate
        })
        .OrderByDescending(s => s.TotalUsageCount)
        .ToList();
    }

    public async Task<List<ToolPerformanceStats>> GetToolPerformanceStatsAsync(int days = 30)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days);

        var dailyStats = await _dailyStatsRepository.GetStatisticsByDateRangeAsync(startDate, endDate);

        return dailyStats
            .GroupBy(s => s.ToolName)
            .Select(g => new ToolPerformanceStats
            {
                ToolName = g.Key,
                AverageExecutionTime = g.Average(s => s.AverageExecutionTimeMs),
                MinExecutionTime = g.Min(s => s.MinExecutionTimeMs),
                MaxExecutionTime = g.Max(s => s.MaxExecutionTimeMs),
                MedianExecutionTime = CalculateMedian(g.Select(s => s.AverageExecutionTimeMs).ToArray()),
                TotalCalls = g.Sum(s => s.TotalUsageCount)
            })
            .OrderByDescending(p => p.TotalCalls)
            .ToList();
    }

    public async Task<List<RecentActivityItem>> GetRecentActivitiesAsync(int count = 20)
    {
        var recentRecords = await _toolUsageRepository.GetRecentRecordsAsync(count);

        return recentRecords.Select(r => new RecentActivityItem
        {
            ToolName = r.ToolName,
            Timestamp = r.StartTime,
            IsSuccess = r.IsSuccess,
            ExecutionTime = r.DurationMs,
            SessionId = r.SessionId,
            ErrorMessage = r.ErrorMessage
        }).ToList();
    }

    public async Task<Dictionary<int, Dictionary<int, int>>> GetUsageHeatmapDataAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var records = await _toolUsageRepository.GetRecordsByDateRangeAsync(startDate, DateTime.UtcNow);

        var heatmapData = new Dictionary<int, Dictionary<int, int>>();

        // 初始化7天24小时的数据结构
        for (int day = 0; day < days; day++)
        {
            heatmapData[day] = new Dictionary<int, int>();
            for (int hour = 0; hour < 24; hour++)
            {
                heatmapData[day][hour] = 0;
            }
        }

        // 填充实际数据
        foreach (var record in records)
        {
            var dayIndex = (int)(DateTime.UtcNow.Date - record.StartTime.Date).TotalDays;
            if (dayIndex >= 0 && dayIndex < days)
            {
                var hour = record.StartTime.Hour;
                heatmapData[dayIndex][hour]++;
            }
        }

        return heatmapData;
    }

    public async Task<List<ToolRankingItem>> GetToolRankingAsync(int topCount = 10, int days = 30)
    {
        var mostActiveTools = await _dailyStatsRepository.GetMostActiveToolsAsync(topCount, days);
        var summary = await _dailyStatsRepository.GetToolSummaryAsync(days);

        return mostActiveTools.Select((tool, index) => new ToolRankingItem
        {
            Rank = index + 1,
            ToolName = tool.toolName,
            UsageCount = tool.usageCount,
            SuccessRate = summary.ContainsKey(tool.toolName) ? summary[tool.toolName].successRate : 0,
            AverageExecutionTime = GetAverageExecutionTimeForTool(tool.toolName, days).Result,
            TrendChange = 0 // TODO: 实现趋势变化计算
        }).ToList();
    }

    private async Task<double> GetAverageExecutionTimeForTool(string toolName, int days)
    {
        var stats = await _dailyStatsRepository.GetStatisticsByToolNameAsync(toolName, null, days);
        return stats.Any() ? stats.Average(s => s.AverageExecutionTimeMs) : 0;
    }

    private static double CalculateMedian(double[] values)
    {
        if (values.Length == 0) return 0;
        
        Array.Sort(values);
        int middle = values.Length / 2;
        
        return values.Length % 2 == 0 
            ? (values[middle - 1] + values[middle]) / 2.0 
            : values[middle];
    }

    // =================== 客户端连接统计相关方法实现 ===================

    public async Task<string> RecordClientConnectionAsync(string clientName, string? clientVersion = null, 
        string? clientTitle = null, string? ipAddress = null, string? userAgent = null,
        string? protocolVersion = null, string? clientCapabilities = null)
    {
        var sessionId = $"client_{Guid.NewGuid():N}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        
        var record = new ClientConnectionRecord
        {
            ClientName = clientName,
            ClientVersion = clientVersion,
            ClientTitle = clientTitle,
            SessionId = sessionId,
            ConnectionTime = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ProtocolVersion = protocolVersion,
            ClientCapabilities = clientCapabilities,
            Status = ClientConnectionStatus.Connected
        };

        await _clientConnectionRepository.AddAsync(record);

        // 异步生成或更新当日客户端统计
        _ = Task.Run(async () =>
        {
            try
            {
                var date = DateOnly.FromDateTime(record.ConnectionTime);
                await _dailyClientStatsRepository.GenerateDailyClientStatisticsAsync(date);
            }
            catch
            {
                // 记录日志但不影响主流程
            }
        });

        return sessionId;
    }

    public async Task UpdateClientConnectionStatusAsync(string sessionId, ClientConnectionStatus status, 
        DateTime? disconnectionTime = null, string? errorMessage = null)
    {
        var record = await _clientConnectionRepository.GetBySessionIdAsync(sessionId);
        if (record != null)
        {
            record.Status = status;
            record.DisconnectionTime = disconnectionTime ?? DateTime.UtcNow;
            record.ErrorMessage = errorMessage;
            
            if (record.DisconnectionTime.HasValue)
            {
                record.ConnectionDurationSeconds = (long)(record.DisconnectionTime.Value - record.ConnectionTime).TotalSeconds;
            }

            await _clientConnectionRepository.UpdateAsync(record);

            // 异步更新当日统计
            _ = Task.Run(async () =>
            {
                try
                {
                    var date = DateOnly.FromDateTime(record.ConnectionTime);
                    await _dailyClientStatsRepository.GenerateDailyClientStatisticsAsync(date);
                }
                catch
                {
                    // 记录日志但不影响主流程
                }
            });
        }
    }

    public async Task IncrementClientToolUsageAsync(string sessionId)
    {
        var record = await _clientConnectionRepository.GetBySessionIdAsync(sessionId);
        // 只有在有UserAgent的客户端记录中才增加工具使用计数
        if (record != null && !string.IsNullOrEmpty(record.UserAgent))
        {
            record.ToolUsageCount++;
            await _clientConnectionRepository.UpdateAsync(record);
        }
    }

    public async Task<ClientOverviewStatistics> GetClientOverviewStatisticsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfToday = today.ToDateTime(TimeOnly.MinValue);
        var endOfToday = today.AddDays(1).ToDateTime(TimeOnly.MinValue);

        // 获取总连接次数
        var totalConnections = await _clientConnectionRepository.GetTotalConnectionCountAsync();

        // 获取今日连接记录
        var todayRecords = await _clientConnectionRepository.GetRecordsByDateRangeAsync(startOfToday, endOfToday);

        // 获取活跃客户端数量（最近30天有连接的客户端）
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentClientsStats = await _clientConnectionRepository.GetActiveClientsStatsAsync(thirtyDaysAgo);
        var activeClientsCount = recentClientsStats.Count;

        // 获取最近7天的记录计算平均成功率和连接时长
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentRecords = await _clientConnectionRepository.GetRecordsByDateRangeAsync(sevenDaysAgo, DateTime.UtcNow);

        var averageSuccessRate = recentRecords.Count > 0 
            ? (double)recentRecords.Count(r => r.Status == ClientConnectionStatus.Connected || 
                                             r.Status == ClientConnectionStatus.Disconnected) / recentRecords.Count * 100 
            : 0;

        var averageConnectionDuration = recentRecords
            .Where(r => r.ConnectionDurationSeconds.HasValue)
            .Select(r => r.ConnectionDurationSeconds!.Value)
            .DefaultIfEmpty(0)
            .Average();

        // 获取今日成功率
        var todaySuccessRate = todayRecords.Count > 0 
            ? (double)todayRecords.Count(r => r.Status == ClientConnectionStatus.Connected || 
                                             r.Status == ClientConnectionStatus.Disconnected) / todayRecords.Count * 100 
            : 0;

        // 获取今日独立会话数
        var uniqueSessionsToday = todayRecords
            .Where(r => !string.IsNullOrEmpty(r.SessionId))
            .Select(r => r.SessionId)
            .Distinct()
            .Count();

        // 获取最活跃客户端
        var mostActiveClient = recentClientsStats.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "无";

        return new ClientOverviewStatistics
        {
            TotalConnections = totalConnections,
            ActiveClientsCount = activeClientsCount,
            AverageConnectionSuccessRate = averageSuccessRate,
            AverageConnectionDuration = averageConnectionDuration,
            TodayConnectionsCount = todayRecords.Count,
            TodaySuccessRate = todaySuccessRate,
            UniqueSessionsToday = uniqueSessionsToday,
            MostActiveClient = mostActiveClient
        };
    }

    public async Task<List<ClientTrendDataPoint>> GetClientConnectionTrendAsync(int days = 30)
    {
        var trendData = await _dailyClientStatsRepository.GetClientConnectionTrendAsync(days);

        return trendData.Select(item => new ClientTrendDataPoint
        {
            Date = item.date.ToDateTime(TimeOnly.MinValue),
            ClientConnections = item.clientConnections,
            TotalConnections = item.clientConnections.Values.Sum(),
            // 这里简化处理，更详细的成功率需要额外计算
            SuccessfulConnections = item.clientConnections.Values.Sum(),
            FailedConnections = 0,
            SuccessRate = 100, // 简化处理
            AverageConnectionDuration = 0 // 需要额外查询计算
        }).ToList();
    }

    public async Task<List<ClientDistribution>> GetClientDistributionAsync(int days = 30)
    {
        var summary = await _dailyClientStatsRepository.GetClientSummaryAsync(days);
        var totalConnections = summary.Values.Sum(v => v.totalConnections);

        if (totalConnections == 0) return new List<ClientDistribution>();

        var colors = new[] { "#8884d8", "#82ca9d", "#ffc658", "#ff7c7c", "#8dd1e1", "#d084d0", "#87ceeb", "#ffa07a" };

        return summary.Select((kvp, index) => new ClientDistribution
        {
            ClientName = kvp.Key,
            ConnectionCount = kvp.Value.totalConnections,
            Percentage = (double)kvp.Value.totalConnections / totalConnections * 100,
            Color = colors[index % colors.Length]
        })
        .OrderByDescending(d => d.ConnectionCount)
        .ToList();
    }

    public async Task<List<ClientVersionDistribution>> GetClientVersionDistributionAsync(int days = 30)
    {
        var versionDistribution = await _clientConnectionRepository.GetClientVersionDistributionAsync(days);
        var result = new List<ClientVersionDistribution>();
        var colors = new[] { "#8884d8", "#82ca9d", "#ffc658", "#ff7c7c", "#8dd1e1", "#d084d0" };
        
        var totalConnections = versionDistribution.Values.SelectMany(v => v.Values).Sum();
        if (totalConnections == 0) return result;

        var colorIndex = 0;
        foreach (var client in versionDistribution)
        {
            foreach (var version in client.Value)
            {
                result.Add(new ClientVersionDistribution
                {
                    ClientName = client.Key,
                    Version = version.Key,
                    ConnectionCount = version.Value,
                    Percentage = (double)version.Value / totalConnections * 100,
                    Color = colors[colorIndex % colors.Length]
                });
                colorIndex++;
            }
        }

        return result.OrderByDescending(d => d.ConnectionCount).ToList();
    }

    public async Task<List<RecentClientActivity>> GetRecentClientActivitiesAsync(int count = 20)
    {
        var recentConnections = await _clientConnectionRepository.GetRecentConnectionsAsync(count);

        return recentConnections.Select(r => new RecentClientActivity
        {
            ClientName = r.ClientName,
            ClientVersion = r.ClientVersion,
            ConnectionTime = r.ConnectionTime,
            Status = r.Status,
            ConnectionDuration = r.ConnectionDurationSeconds,
            SessionId = r.SessionId ?? string.Empty,
            ToolUsageCount = r.ToolUsageCount,
            ErrorMessage = r.ErrorMessage
        }).ToList();
    }

    public async Task<List<ClientRankingItem>> GetClientRankingAsync(int topCount = 10, int days = 30)
    {
        var mostActiveClients = await _dailyClientStatsRepository.GetMostActiveClientsAsync(topCount, days);

        return mostActiveClients.Select((client, index) => new ClientRankingItem
        {
            Rank = index + 1,
            ClientName = client.clientName,
            ConnectionCount = client.connectionCount,
            SuccessRate = client.successRate,
            AverageConnectionDuration = 0, // 需要额外计算
            TotalToolUsages = 0, // 需要额外计算
            TrendChange = 0 // 实现趋势变化计算
        }).ToList();
    }

    public async Task<Dictionary<int, Dictionary<int, Dictionary<string, int>>>> GetClientConnectionHeatmapAsync(int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var records = await _clientConnectionRepository.GetRecordsByDateRangeAsync(startDate, DateTime.UtcNow);

        var heatmapData = new Dictionary<int, Dictionary<int, Dictionary<string, int>>>();

        // 初始化7天24小时的数据结构
        for (int day = 0; day < days; day++)
        {
            heatmapData[day] = new Dictionary<int, Dictionary<string, int>>();
            for (int hour = 0; hour < 24; hour++)
            {
                heatmapData[day][hour] = new Dictionary<string, int>();
            }
        }

        // 填充实际数据
        foreach (var record in records)
        {
            var dayIndex = (int)(DateTime.UtcNow.Date - record.ConnectionTime.Date).TotalDays;
            if (dayIndex >= 0 && dayIndex < days)
            {
                var hour = record.ConnectionTime.Hour;
                if (!heatmapData[dayIndex][hour].ContainsKey(record.ClientName))
                {
                    heatmapData[dayIndex][hour][record.ClientName] = 0;
                }
                heatmapData[dayIndex][hour][record.ClientName]++;
            }
        }

        return heatmapData;
    }
}
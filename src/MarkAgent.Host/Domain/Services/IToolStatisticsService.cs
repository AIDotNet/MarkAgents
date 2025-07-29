using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Services;

/// <summary>
/// 工具统计服务接口 - 支持多种展示方式和图表数据
/// </summary>
public interface IToolStatisticsService
{
    /// <summary>
    /// 记录工具使用（在工具执行时调用）
    /// </summary>
    Task RecordToolUsageAsync(string toolName, DateTime startTime, DateTime endTime, 
        bool isSuccess, string? errorMessage = null, int inputSize = 0, int outputSize = 0,
        string? parametersJson = null, string? sessionId = null, 
        string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// 获取概览统计数据（用于仪表板卡片）
    /// </summary>
    Task<OverviewStatistics> GetOverviewStatisticsAsync();

    /// <summary>
    /// 获取工具使用趋势数据（用于趋势图表）
    /// </summary>
    Task<List<TrendDataPoint>> GetUsageTrendDataAsync(int days = 30);

    /// <summary>
    /// 获取工具使用分布数据（用于饼图/环形图）
    /// </summary>
    Task<List<ToolUsageDistribution>> GetToolUsageDistributionAsync(int days = 30);

    /// <summary>
    /// 获取成功率统计数据（用于条形图）
    /// </summary>
    Task<List<ToolSuccessRateStats>> GetToolSuccessRateStatsAsync(int days = 30);

    /// <summary>
    /// 获取性能统计数据（用于性能图表）
    /// </summary>
    Task<List<ToolPerformanceStats>> GetToolPerformanceStatsAsync(int days = 30);

    /// <summary>
    /// 获取实时活动数据（用于活动时间线）
    /// </summary>
    Task<List<RecentActivityItem>> GetRecentActivitiesAsync(int count = 20);

    /// <summary>
    /// 获取热力图数据（按小时统计使用情况）
    /// </summary>
    Task<Dictionary<int, Dictionary<int, int>>> GetUsageHeatmapDataAsync(int days = 7);

    /// <summary>
    /// 获取工具使用排行榜
    /// </summary>
    Task<List<ToolRankingItem>> GetToolRankingAsync(int topCount = 10, int days = 30);

    // =================== 客户端连接统计相关方法 ===================

    /// <summary>
    /// 记录客户端连接（在ConfigureSessionOptions中调用）
    /// </summary>
    Task<string> RecordClientConnectionAsync(string clientName, string? clientVersion = null, 
        string? clientTitle = null, string? ipAddress = null, string? userAgent = null,
        string? protocolVersion = null, string? clientCapabilities = null);

    /// <summary>
    /// 更新客户端连接状态（断开连接时调用）
    /// </summary>
    Task UpdateClientConnectionStatusAsync(string sessionId, ClientConnectionStatus status, 
        DateTime? disconnectionTime = null, string? errorMessage = null);

    /// <summary>
    /// 增加客户端连接的工具使用计数
    /// </summary>
    Task IncrementClientToolUsageAsync(string sessionId);

    /// <summary>
    /// 获取客户端连接概览统计
    /// </summary>
    Task<ClientOverviewStatistics> GetClientOverviewStatisticsAsync();

    /// <summary>
    /// 获取客户端连接趋势数据
    /// </summary>
    Task<List<ClientTrendDataPoint>> GetClientConnectionTrendAsync(int days = 30);

    /// <summary>
    /// 获取客户端分布数据
    /// </summary>
    Task<List<ClientDistribution>> GetClientDistributionAsync(int days = 30);

    /// <summary>
    /// 获取客户端版本分布
    /// </summary>
    Task<List<ClientVersionDistribution>> GetClientVersionDistributionAsync(int days = 30);

    /// <summary>
    /// 获取最近的客户端连接活动
    /// </summary>
    Task<List<RecentClientActivity>> GetRecentClientActivitiesAsync(int count = 20);

    /// <summary>
    /// 获取客户端活跃度排行
    /// </summary>
    Task<List<ClientRankingItem>> GetClientRankingAsync(int topCount = 10, int days = 30);

    /// <summary>
    /// 获取客户端连接热力图数据（按小时统计）
    /// </summary>
    Task<Dictionary<int, Dictionary<int, Dictionary<string, int>>>> GetClientConnectionHeatmapAsync(int days = 7);
}

/// <summary>
/// 概览统计数据
/// </summary>
public class OverviewStatistics
{
    public int TotalUsageCount { get; set; }
    public int ActiveToolsCount { get; set; }
    public double AverageSuccessRate { get; set; }
    public double AverageExecutionTime { get; set; }
    public int TodayUsageCount { get; set; }
    public double TodaySuccessRate { get; set; }
    public int UniqueSessions { get; set; }
    public string MostUsedTool { get; set; } = string.Empty;
}

/// <summary>
/// 趋势数据点
/// </summary>
public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public int TotalUsage { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageExecutionTime { get; set; }
}

/// <summary>
/// 工具使用分布
/// </summary>
public class ToolUsageDistribution
{
    public string ToolName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 工具成功率统计
/// </summary>
public class ToolSuccessRateStats
{
    public string ToolName { get; set; } = string.Empty;
    public int TotalUsageCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double SuccessRate { get; set; }
}

/// <summary>
/// 工具性能统计
/// </summary>
public class ToolPerformanceStats
{
    public string ToolName { get; set; } = string.Empty;
    public double AverageExecutionTime { get; set; }
    public long MinExecutionTime { get; set; }
    public long MaxExecutionTime { get; set; }
    public double MedianExecutionTime { get; set; }
    public int TotalCalls { get; set; }
}

/// <summary>
/// 最近活动项目
/// </summary>
public class RecentActivityItem
{
    public string ToolName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
    public long ExecutionTime { get; set; }
    public string? SessionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 工具排行榜项目
/// </summary>
public class ToolRankingItem
{
    public int Rank { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageExecutionTime { get; set; }
    public int TrendChange { get; set; } // 与上期相比的变化
}

// =================== 客户端统计相关数据传输对象 ===================

/// <summary>
/// 客户端概览统计数据
/// </summary>
public class ClientOverviewStatistics
{
    public int TotalConnections { get; set; }
    public int ActiveClientsCount { get; set; }
    public double AverageConnectionSuccessRate { get; set; }
    public double AverageConnectionDuration { get; set; }
    public int TodayConnectionsCount { get; set; }
    public double TodaySuccessRate { get; set; }
    public int UniqueSessionsToday { get; set; }
    public string MostActiveClient { get; set; } = string.Empty;
}

/// <summary>
/// 客户端连接趋势数据点
/// </summary>
public class ClientTrendDataPoint
{
    public DateTime Date { get; set; }
    public Dictionary<string, int> ClientConnections { get; set; } = new();
    public int TotalConnections { get; set; }
    public int SuccessfulConnections { get; set; }
    public int FailedConnections { get; set; }
    public double SuccessRate { get; set; }
    public double AverageConnectionDuration { get; set; }
}

/// <summary>
/// 客户端分布数据
/// </summary>
public class ClientDistribution
{
    public string ClientName { get; set; } = string.Empty;
    public int ConnectionCount { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 客户端版本分布
/// </summary>
public class ClientVersionDistribution
{
    public string ClientName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int ConnectionCount { get; set; }
    public double Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 最近客户端活动项目
/// </summary>
public class RecentClientActivity
{
    public string ClientName { get; set; } = string.Empty;
    public string? ClientVersion { get; set; }
    public DateTime ConnectionTime { get; set; }
    public ClientConnectionStatus Status { get; set; }
    public long? ConnectionDuration { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int ToolUsageCount { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 客户端排行榜项目
/// </summary>
public class ClientRankingItem
{
    public int Rank { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? ClientVersion { get; set; }
    public int ConnectionCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageConnectionDuration { get; set; }
    public int TotalToolUsages { get; set; }
    public int TrendChange { get; set; } // 与上期相比的变化
}
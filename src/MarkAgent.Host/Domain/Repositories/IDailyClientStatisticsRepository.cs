using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 每日客户端统计仓储接口
/// </summary>
public interface IDailyClientStatisticsRepository
{
    /// <summary>
    /// 添加或更新每日客户端统计
    /// </summary>
    Task<DailyClientStatistics> AddOrUpdateAsync(DailyClientStatistics statistics);

    /// <summary>
    /// 获取指定日期范围内的客户端统计数据
    /// </summary>
    Task<List<DailyClientStatistics>> GetStatisticsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 获取指定客户端的统计数据
    /// </summary>
    Task<List<DailyClientStatistics>> GetStatisticsByClientNameAsync(string clientName, DateOnly? startDate = null, int days = 30);

    /// <summary>
    /// 获取所有客户端的汇总统计
    /// </summary>
    Task<Dictionary<string, (int totalConnections, double successRate, double avgDuration)>> GetClientSummaryAsync(int days = 30);

    /// <summary>
    /// 根据原始连接记录生成每日客户端统计
    /// </summary>
    Task GenerateDailyClientStatisticsAsync(DateOnly date);

    /// <summary>
    /// 获取最活跃的客户端（按连接次数排序）
    /// </summary>
    Task<List<(string clientName, int connectionCount, double successRate)>> GetMostActiveClientsAsync(int topCount = 10, int days = 30);

    /// <summary>
    /// 获取客户端连接趋势数据
    /// </summary>
    Task<List<(DateOnly date, Dictionary<string, int> clientConnections)>> GetClientConnectionTrendAsync(int days = 30);
}
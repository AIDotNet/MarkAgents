using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 每日工具统计仓储接口
/// </summary>
public interface IDailyToolStatisticsRepository
{
    /// <summary>
    /// 添加或更新每日统计
    /// </summary>
    Task<DailyToolStatistics> AddOrUpdateAsync(DailyToolStatistics statistics);

    /// <summary>
    /// 获取指定日期范围内的统计数据
    /// </summary>
    Task<List<DailyToolStatistics>> GetStatisticsByDateRangeAsync(DateOnly startDate, DateOnly endDate);

    /// <summary>
    /// 获取指定工具的统计数据
    /// </summary>
    Task<List<DailyToolStatistics>> GetStatisticsByToolNameAsync(string toolName, DateOnly? startDate = null, int days = 30);

    /// <summary>
    /// 获取所有工具的汇总统计
    /// </summary>
    Task<Dictionary<string, (int totalUsage, double successRate)>> GetToolSummaryAsync(int days = 30);

    /// <summary>
    /// 根据原始记录生成每日统计
    /// </summary>
    Task GenerateDailyStatisticsAsync(DateOnly date);

    /// <summary>
    /// 获取最活跃的工具（按使用次数排序）
    /// </summary>
    Task<List<(string toolName, int usageCount)>> GetMostActiveToolsAsync(int topCount = 10, int days = 30);
}
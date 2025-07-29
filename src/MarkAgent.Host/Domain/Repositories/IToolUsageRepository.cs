using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 工具使用记录仓储接口
/// </summary>
public interface IToolUsageRepository
{
    /// <summary>
    /// 添加工具使用记录
    /// </summary>
    Task<ToolUsageRecord> AddAsync(ToolUsageRecord record);

    /// <summary>
    /// 获取指定时间范围内的使用记录
    /// </summary>
    Task<List<ToolUsageRecord>> GetRecordsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 获取指定工具的使用记录
    /// </summary>
    Task<List<ToolUsageRecord>> GetRecordsByToolNameAsync(string toolName, int skip = 0, int take = 100);

    /// <summary>
    /// 获取总使用次数
    /// </summary>
    Task<int> GetTotalUsageCountAsync();

    /// <summary>
    /// 获取最近的使用记录
    /// </summary>
    Task<List<ToolUsageRecord>> GetRecentRecordsAsync(int count = 10);

    /// <summary>
    /// 获取工具使用统计（按工具名称分组）
    /// </summary>
    Task<Dictionary<string, int>> GetUsageStatsByToolAsync(DateTime? startDate = null, DateTime? endDate = null);
}
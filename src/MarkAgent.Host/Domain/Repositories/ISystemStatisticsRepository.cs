using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 系统统计仓储接口
/// </summary>
public interface ISystemStatisticsRepository : IRepository<SystemStatistics, Guid>
{
    /// <summary>
    /// 根据日期获取系统统计信息
    /// </summary>
    Task<SystemStatistics?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定日期范围的系统统计信息
    /// </summary>
    Task<List<SystemStatistics>> GetByDateRangeAsync(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建或更新系统统计信息
    /// </summary>
    Task<SystemStatistics> CreateOrUpdateAsync(SystemStatistics statistics,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新系统统计数据
    /// </summary>
    Task RefreshSystemStatisticsAsync(DateTime date, CancellationToken cancellationToken = default);
}
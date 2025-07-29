using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 用户统计仓储接口
/// </summary>
public interface IUserStatisticsRepository : IRepository<UserStatistics, Guid>
{
    /// <summary>
    /// 根据用户ID和日期获取统计信息
    /// </summary>
    Task<UserStatistics?> GetByUserAndDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户密钥和日期获取统计信息
    /// </summary>
    Task<UserStatistics?> GetByUserKeyAndDateAsync(Guid userKeyId, DateTime date, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户指定日期范围的统计信息
    /// </summary>
    Task<List<UserStatistics>> GetByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加Todo创建数量
    /// </summary>
    Task IncrementTodosCreatedAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加Todo完成数量
    /// </summary>
    Task IncrementTodosCompletedAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加对话数量
    /// </summary>
    Task IncrementConversationsCountAsync(Guid userId, Guid userKeyId, DateTime date, int count = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加Token使用量
    /// </summary>
    Task IncrementTokenUsageAsync(Guid userId, Guid userKeyId, DateTime date, long tokens, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加API调用次数
    /// </summary>
    Task IncrementApiCallsAsync(Guid userId, Guid userKeyId, DateTime date, long calls = 1, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 增加在线时长
    /// </summary>
    Task IncrementOnlineTimeAsync(Guid userId, Guid userKeyId, DateTime date, int minutes, CancellationToken cancellationToken = default);
}
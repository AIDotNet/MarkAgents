using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// Todo仓储接口
/// </summary>
public interface ITodoRepository : IRepository<Todo, string>
{
    /// <summary>
    /// 根据用户ID获取Todo列表
    /// </summary>
    Task<List<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户密钥获取Todo列表
    /// </summary>
    Task<List<Todo>> GetByUserKeyAsync(string userKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据对话ID获取Todo列表
    /// </summary>
    Task<List<Todo>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据状态获取Todo列表
    /// </summary>
    Task<List<Todo>> GetByStatusAsync(TodoStatus status, Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的待处理Todo数量
    /// </summary>
    Task<int> GetPendingCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的已完成Todo数量
    /// </summary>
    Task<int> GetCompletedCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的Todo统计信息
    /// </summary>
    Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatsByUserAsync(Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户密钥获取Todo统计信息
    /// </summary>
    Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatsByUserKeyAsync(string userKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定日期范围内的Todo创建数量
    /// </summary>
    Task<int> GetCreatedCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定日期范围内的Todo完成数量
    /// </summary>
    Task<int> GetCompletedCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新Todo状态
    /// </summary>
    Task UpdateStatusAsync(string todoId, TodoStatus status, CancellationToken cancellationToken = default);


    /// <summary>
    /// 批量更新Todo状态
    /// </summary>
    Task UpdateStatusBatchAsync(List<string> todoIds, TodoStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量完成用户的所有待处理Todo
    /// </summary>
    Task<int> CompleteAllPendingByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
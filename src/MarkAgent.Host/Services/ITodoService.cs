using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// Todo服务接口
/// </summary>
public interface ITodoService
{
    /// <summary>
    /// 创建Todo
    /// </summary>
    /// <param name="userKey"></param>
    /// <param name="conversationId"></param>
    /// <param name="todos"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> CreateTodoAsync(string userKey, Guid conversationId, List<Todo> todos,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建Todo
    /// </summary>
    Task<(bool Success, string Message, Todo? Todo)> CreateTodoAsync(string userKey, Guid conversationId, string title,
        string? description = null, Priority priority = Priority.Medium, DateTime? dueDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户密钥获取Todo列表
    /// </summary>
    Task<List<Todo>> GetTodosByUserKeyAsync(string userKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据对话ID获取Todo列表
    /// </summary>
    Task<List<Todo>> GetTodosByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据状态获取Todo列表
    /// </summary>
    Task<List<Todo>> GetTodosByStatusAsync(string userKey, TodoStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新Todo状态
    /// </summary>
    Task<(bool Success, string Message)> UpdateTodoStatusAsync(string userKey, string todoId, TodoStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新Todo进度
    /// </summary>
    Task<(bool Success, string Message)> UpdateTodoProgressAsync(string userKey, string todoId, int progress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新Todo信息
    /// </summary>
    Task<(bool Success, string Message)> UpdateTodoAsync(string userKey, string todoId,
        string? content = null, Priority? priority = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除Todo
    /// </summary>
    Task<(bool Success, string Message)> DeleteTodoAsync(string userKey, string todoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量更新Todo状态
    /// </summary>
    Task<(bool Success, string Message, int AffectedCount)> UpdateTodoStatusBatchAsync(string userKey,
        List<string> todoIds, TodoStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户Todo统计信息
    /// </summary>
    Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatisticsAsync(string userKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 完成用户的所有待处理Todo
    /// </summary>
    Task<(bool Success, string Message, int CompletedCount)> CompleteAllPendingAsync(string userKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取Todo详情
    /// </summary>
    Task<Todo?> GetTodoByIdAsync(string userKey, string todoId, CancellationToken cancellationToken = default);
}
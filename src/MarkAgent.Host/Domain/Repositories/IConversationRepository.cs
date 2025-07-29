using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 对话仓储接口
/// </summary>
public interface IConversationRepository : IRepository<Conversation, Guid>
{
    /// <summary>
    /// 根据用户ID获取对话列表
    /// </summary>
    Task<List<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户密钥获取对话列表
    /// </summary>
    Task<List<Conversation>> GetByUserKeyAsync(string userKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据状态获取对话列表
    /// </summary>
    Task<List<Conversation>> GetByStatusAsync(ConversationStatus status, Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的活跃对话
    /// </summary>
    Task<List<Conversation>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户密钥获取活跃对话
    /// </summary>
    Task<Conversation?> GetActiveByUserKeyAsync(string userKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的对话统计信息
    /// </summary>
    Task<(int Total, int Active, int Completed, int Interrupted, int Error)> GetConversationStatsByUserAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定日期范围内的对话数量
    /// </summary>
    Task<int> GetConversationCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 结束对话
    /// </summary>
    Task EndConversationAsync(Guid conversationId, ConversationStatus status = ConversationStatus.Completed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量结束用户的活跃对话
    /// </summary>
    Task EndActiveConversationsByUserAsync(Guid userId, ConversationStatus status = ConversationStatus.Completed,
        CancellationToken cancellationToken = default);
}
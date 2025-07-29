using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// MCP服务接口
/// </summary>
public interface IMcpService
{
    /// <summary>
    /// 创建新的对话会话
    /// </summary>
    Task<(bool Success, string Message, Conversation? Conversation)> CreateConversationAsync(string userKey, string? title = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户密钥获取活跃对话
    /// </summary>
    Task<Conversation?> GetActiveConversationAsync(string userKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 结束对话
    /// </summary>
    Task<(bool Success, string Message)> EndConversationAsync(string userKey, Guid conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新对话Token使用量
    /// </summary>
    Task UpdateConversationTokenUsageAsync(Guid conversationId, long tokenUsage, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户对话列表
    /// </summary>
    Task<List<Conversation>> GetUserConversationsAsync(string userKey, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户对话统计信息
    /// </summary>
    Task<(int Total, int Active, int Completed, int Interrupted, int Error)> GetConversationStatisticsAsync(string userKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 配置用户的MCP工具
    /// </summary>
    Task<(bool Success, string Message)> ConfigureToolsAsync(string userKey, List<McpToolConfig> toolConfigs, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户启用的MCP工具列表
    /// </summary>
    Task<List<McpToolConfig>> GetEnabledToolsAsync(string userKey, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 切换工具启用状态
    /// </summary>
    Task<(bool Success, string Message)> ToggleToolAsync(string userKey, string toolName, bool isEnabled, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 记录工具使用
    /// </summary>
    Task RecordToolUsageAsync(string userKey, Guid conversationId, string toolName, string? parameters, string? result, ExecutionStatus status, int executionTimeMs = 0, int tokensUsed = 0, string? errorMessage = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取工具使用统计
    /// </summary>
    Task<Dictionary<string, int>> GetToolUsageStatisticsAsync(string userKey, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
} 
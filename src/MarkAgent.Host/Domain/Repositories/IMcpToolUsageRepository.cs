using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// MCP工具使用记录仓储接口
/// </summary>
public interface IMcpToolUsageRepository : IRepository<McpToolUsage, Guid>
{
    /// <summary>
    /// 根据用户ID获取使用记录
    /// </summary>
    Task<List<McpToolUsage>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户密钥获取使用记录
    /// </summary>
    Task<List<McpToolUsage>> GetByUserKeyIdAsync(Guid userKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据对话ID获取使用记录
    /// </summary>
    Task<List<McpToolUsage>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据工具名称获取使用记录
    /// </summary>
    Task<List<McpToolUsage>> GetByToolNameAsync(string toolName, Guid? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取指定日期范围内的使用记录
    /// </summary>
    Task<List<McpToolUsage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的工具使用统计
    /// </summary>
    Task<Dictionary<string, int>> GetToolUsageStatsByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取工具使用频率统计
    /// </summary>
    Task<Dictionary<string, int>> GetToolPopularityStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的平均执行时间统计
    /// </summary>
    Task<Dictionary<string, double>> GetAverageExecutionTimeByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取工具错误率统计
    /// </summary>
    Task<Dictionary<string, double>> GetToolErrorRatesAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 记录工具使用
    /// </summary>
    Task<McpToolUsage> RecordUsageAsync(McpToolUsage usage, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的Token使用量统计
    /// </summary>
    Task<long> GetTokenUsageByUserAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取系统总的工具调用次数
    /// </summary>
    Task<long> GetTotalToolCallsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}
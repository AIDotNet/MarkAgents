using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// MCP工具配置仓储接口
/// </summary>
public interface IMcpToolConfigRepository : IRepository<McpToolConfig, Guid>
{
    /// <summary>
    /// 根据用户密钥ID获取工具配置列表
    /// </summary>
    Task<List<McpToolConfig>> GetByUserKeyIdAsync(Guid userKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户密钥和工具名称获取配置
    /// </summary>
    Task<McpToolConfig?> GetByUserKeyAndToolAsync(Guid userKeyId, string toolName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户密钥获取启用的工具列表
    /// </summary>
    Task<List<McpToolConfig>> GetEnabledByUserKeyAsync(Guid userKeyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据工具名称获取所有配置
    /// </summary>
    Task<List<McpToolConfig>> GetByToolNameAsync(string toolName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查用户密钥是否已配置指定工具
    /// </summary>
    Task<bool> IsToolConfiguredAsync(Guid userKeyId, string toolName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 切换工具启用状态
    /// </summary>
    Task ToggleToolAsync(Guid userKeyId, string toolName, bool isEnabled, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 批量更新工具配置
    /// </summary>
    Task UpdateToolConfigsAsync(Guid userKeyId, List<McpToolConfig> configs, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新工具使用统计
    /// </summary>
    Task UpdateUsageStatsAsync(Guid configId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户密钥的工具统计信息
    /// </summary>
    Task<(int TotalTools, int EnabledTools, long TotalUsage)> GetToolStatsByUserKeyAsync(Guid userKeyId, CancellationToken cancellationToken = default);
}
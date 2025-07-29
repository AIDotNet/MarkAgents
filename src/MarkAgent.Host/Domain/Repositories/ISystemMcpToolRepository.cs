using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 系统MCP工具存储库接口
/// </summary>
public interface ISystemMcpToolRepository : IRepository<SystemMcpTool, Guid>
{
    /// <summary>
    /// 获取所有启用的系统工具
    /// </summary>
    Task<List<SystemMcpTool>> GetEnabledToolsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据分类获取工具列表
    /// </summary>
    Task<List<SystemMcpTool>> GetToolsByCategoryAsync(string category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据工具名称获取工具
    /// </summary>
    Task<SystemMcpTool?> GetByToolNameAsync(string toolName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据标签搜索工具
    /// </summary>
    Task<List<SystemMcpTool>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 搜索工具（按名称或描述）
    /// </summary>
    Task<List<SystemMcpTool>> SearchToolsAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取工具统计信息
    /// </summary>
    Task<(int TotalTools, int EnabledTools, int BuiltInTools)> GetToolStatisticsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新工具使用计数
    /// </summary>
    Task UpdateUsageCountAsync(Guid toolId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取最受欢迎的工具
    /// </summary>
    Task<List<SystemMcpTool>> GetPopularToolsAsync(int count = 10, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查工具名称是否已存在
    /// </summary>
    Task<bool> ToolNameExistsAsync(string toolName, Guid? excludeId = null, CancellationToken cancellationToken = default);
} 
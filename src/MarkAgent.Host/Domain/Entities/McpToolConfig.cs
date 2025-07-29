using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// MCP工具配置实体
/// </summary>
public class McpToolConfig : BaseEntity<Guid>
{
    /// <summary>
    /// 用户密钥ID
    /// </summary>
    [Required]
    public Guid UserKeyId { get; set; }
    
    /// <summary>
    /// 工具名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// 工具显示名称
    /// </summary>
    [StringLength(200)]
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// 工具描述
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 工具配置（JSON格式）
    /// </summary>
    public string? Configuration { get; set; }
    
    /// <summary>
    /// 工具版本
    /// </summary>
    [StringLength(50)]
    public string? Version { get; set; }
    
    /// <summary>
    /// 权限配置（JSON格式）
    /// </summary>
    public string? Permissions { get; set; }
    
    /// <summary>
    /// 使用次数
    /// </summary>
    public long UsageCount { get; set; } = 0;
    
    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// 导航属性：所属用户密钥
    /// </summary>
    public virtual UserKey UserKey { get; set; } = null!;
}

/// <summary>
/// 执行状态枚举
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// 成功
    /// </summary>
    Success = 1,
    
    /// <summary>
    /// 失败
    /// </summary>
    Failed = 2,
    
    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 3,
    
    /// <summary>
    /// 权限拒绝
    /// </summary>
    PermissionDenied = 4
} 
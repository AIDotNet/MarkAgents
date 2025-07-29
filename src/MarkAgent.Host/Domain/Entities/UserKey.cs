using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 用户API密钥实体
/// </summary>
public class UserKey : BaseEntity<Guid>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// API密钥，以sk-开头
    /// </summary>
    [Required]
    [StringLength(64)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 密钥名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 密钥描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 是否为默认密钥
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// 密钥状态
    /// </summary>
    public KeyStatus Status { get; set; } = KeyStatus.Active;

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// 使用次数
    /// </summary>
    public long UsageCount { get; set; } = 0;

    /// <summary>
    /// MCP工具配置（JSON格式），存储启用的工具列表和配置
    /// </summary>
    public List<McpToolsConfig>? McpToolsConfig { get; set; } = new List<McpToolsConfig>();

    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 导航属性：使用此密钥的对话
    /// </summary>
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    /// <summary>
    /// 导航属性：此密钥关联的MCP工具配置
    /// </summary>
    public virtual ICollection<McpToolConfig> McpToolConfigs { get; set; } = new List<McpToolConfig>();
}

public class McpToolsConfig
{
    public required string ToolName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// 密钥状态枚举
/// </summary>
public enum KeyStatus
{
    /// <summary>
    /// 活跃
    /// </summary>
    Active = 1,

    /// <summary>
    /// 已禁用
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// 已过期
    /// </summary>
    Expired = 3
}
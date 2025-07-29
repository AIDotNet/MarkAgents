using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// MCP工具使用记录实体
/// </summary>
public class McpToolUsage : BaseEntity<Guid>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 用户密钥ID
    /// </summary>
    [Required]
    public Guid UserKeyId { get; set; }
    
    /// <summary>
    /// 对话ID
    /// </summary>
    [Required]
    public Guid ConversationId { get; set; }
    
    /// <summary>
    /// 工具名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// 调用参数（JSON格式）
    /// </summary>
    public string? Parameters { get; set; }
    
    /// <summary>
    /// 调用结果（JSON格式）
    /// </summary>
    public string? Result { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Success;
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public int ExecutionTimeMs { get; set; } = 0;
    
    /// <summary>
    /// Token使用量
    /// </summary>
    public int TokensUsed { get; set; } = 0;
    
    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// 导航属性：使用的密钥
    /// </summary>
    public virtual UserKey UserKey { get; set; } = null!;
    
    /// <summary>
    /// 导航属性：所属对话
    /// </summary>
    public virtual Conversation Conversation { get; set; } = null!;
}
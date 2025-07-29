using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// Todo实体 - 任务管理
/// </summary>
public class Todo : BaseEntity<string>
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
    /// 任务内容
    /// </summary>
    [StringLength(1000)]
    public string? Content { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    /// <summary>
    /// 优先级
    /// </summary>
    public Priority Priority { get; set; }


    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// 排序索引
    /// </summary>
    public int SortOrder { get; set; } = 0;

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
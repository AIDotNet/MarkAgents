using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 对话实体 - 代表一次MCP工具服务的会话
/// </summary>
public class Conversation : BaseEntity<Guid>, IAsyncDisposable
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
    
    [Required]
    public required string SessionId { get; set; }

    /// <summary>
    /// 会话标题
    /// </summary>
    [StringLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// 会话状态
    /// </summary>
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;

    /// <summary>
    /// 会话开始时间
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 会话结束时间
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// 会话持续时间（秒）
    /// </summary>
    public int? DurationSeconds { get; set; }
    
    /// <summary>
    /// 会话元数据（JSON格式）
    /// </summary
    public string? Metadata { get; set; }
    
    /// <summary>
    /// 客户端信息
    /// </summary>
    public string? Client { get; set; }
    
    /// <summary>
    /// 客户端版本
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// 导航属性：使用的密钥
    /// </summary>
    public virtual UserKey UserKey { get; set; } = null!;

    /// <summary>
    /// 导航属性：此会话的Todo列表
    /// </summary>
    public virtual ICollection<Todo> Todos { get; set; } = new List<Todo>();

    /// <summary>
    /// 异步释放资源，标记会话结束
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Status == ConversationStatus.Active)
        {
            Status = ConversationStatus.Completed;
            EndedAt = DateTime.UtcNow;

            if (StartedAt != default)
            {
                DurationSeconds = (int)(EndedAt.Value - StartedAt).TotalSeconds;
            }
        }

        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }
}

/// <summary>
/// 对话状态枚举
/// </summary>
public enum ConversationStatus
{
    /// <summary>
    /// 活跃中
    /// </summary>
    Active = 1,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,

    /// <summary>
    /// 已中断
    /// </summary>
    Interrupted = 3,

    /// <summary>
    /// 出错
    /// </summary>
    Error = 4
}
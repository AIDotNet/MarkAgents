using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 用户统计实体
/// </summary>
public class UserStatistics : BaseEntity<Guid>
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
    /// 统计日期
    /// </summary>
    [Required]
    public DateTime StatDate { get; set; }
    
    /// <summary>
    /// Todo创建总数
    /// </summary>
    public int TodosCreated { get; set; } = 0;
    
    /// <summary>
    /// Todo完成总数
    /// </summary>
    public int TodosCompleted { get; set; } = 0;
    
    /// <summary>
    /// 对话总数
    /// </summary>
    public int ConversationsCount { get; set; } = 0;
    
    /// <summary>
    /// 总Token使用量
    /// </summary>
    public long TotalTokensUsed { get; set; } = 0;
    
    /// <summary>
    /// API调用次数
    /// </summary>
    public long ApiCallsCount { get; set; } = 0;
    
    /// <summary>
    /// 总在线时长（分钟）
    /// </summary>
    public int TotalOnlineMinutes { get; set; } = 0;
    
    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    public virtual User User { get; set; } = null!;
    
    /// <summary>
    /// 导航属性：相关密钥
    /// </summary>
    public virtual UserKey UserKey { get; set; } = null!;
}

/// <summary>
/// 系统统计实体
/// </summary>
public class SystemStatistics : BaseEntity<Guid>
{
    /// <summary>
    /// 统计日期
    /// </summary>
    [Required]
    public DateTime StatDate { get; set; }
    
    /// <summary>
    /// 总用户数
    /// </summary>
    public int TotalUsers { get; set; } = 0;
    
    /// <summary>
    /// 活跃用户数
    /// </summary>
    public int ActiveUsers { get; set; } = 0;
    
    /// <summary>
    /// 新注册用户数
    /// </summary>
    public int NewUsers { get; set; } = 0;
    
    /// <summary>
    /// 总密钥数
    /// </summary>
    public int TotalUserKeys { get; set; } = 0;
    
    /// <summary>
    /// 总Todo数
    /// </summary>
    public int TotalTodos { get; set; } = 0;
    
    /// <summary>
    /// 完成的Todo数
    /// </summary>
    public int CompletedTodos { get; set; } = 0;
    
    /// <summary>
    /// 总对话数
    /// </summary>
    public int TotalConversations { get; set; } = 0;
    
    /// <summary>
    /// 系统总Token使用量
    /// </summary>
    public long TotalTokensUsed { get; set; } = 0;
    
    /// <summary>
    /// 系统总API调用次数
    /// </summary>
    public long TotalApiCalls { get; set; } = 0;
} 
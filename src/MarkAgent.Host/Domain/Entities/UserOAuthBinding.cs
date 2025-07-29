using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 用户OAuth绑定实体
/// </summary>
public class UserOAuthBinding : BaseEntity<Guid>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// OAuth提供商
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// 提供商用户ID
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ProviderUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// 提供商用户名/昵称
    /// </summary>
    [StringLength(100)]
    public string? ProviderUserName { get; set; }
    
    /// <summary>
    /// 提供商邮箱
    /// </summary>
    [StringLength(256)]
    [EmailAddress]
    public string? ProviderEmail { get; set; }
    
    /// <summary>
    /// 提供商头像URL
    /// </summary>
    [StringLength(500)]
    public string? ProviderAvatarUrl { get; set; }
    
    /// <summary>
    /// 访问令牌
    /// </summary>
    [StringLength(1000)]
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// 刷新令牌
    /// </summary>
    [StringLength(1000)]
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// 令牌过期时间
    /// </summary>
    public DateTime? TokenExpiredAt { get; set; }
    
    /// <summary>
    /// 绑定时间
    /// </summary>
    [Required]
    public DateTime BindTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 最后同步时间
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// 绑定状态
    /// </summary>
    public OAuthBindingStatus Status { get; set; } = OAuthBindingStatus.Active;
    
    /// <summary>
    /// 额外信息（JSON格式）
    /// </summary>
    public string? ExtraInfo { get; set; }
    
    /// <summary>
    /// 导航属性：所属用户
    /// </summary>
    public virtual User User { get; set; } = null!;
}

/// <summary>
/// OAuth绑定状态枚举
/// </summary>
public enum OAuthBindingStatus
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
    /// 令牌已过期
    /// </summary>
    TokenExpired = 3,
    
    /// <summary>
    /// 已撤销
    /// </summary>
    Revoked = 4
}

/// <summary>
/// OAuth提供商常量
/// </summary>
public static class OAuthProviders
{
    public const string GitHub = "github";
    public const string Gitee = "gitee";
    public const string Google = "google";
    public const string Microsoft = "microsoft";
    public const string WeChat = "wechat";
    public const string QQ = "qq";
} 
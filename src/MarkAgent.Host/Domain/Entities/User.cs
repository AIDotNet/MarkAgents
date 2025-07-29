using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User : BaseEntity<Guid>
{
    /// <summary>
    /// 邮箱（登录账号）
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// 密码哈希
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱验证状态
    /// </summary>
    public bool EmailConfirmed { get; set; } = false;
    
    /// <summary>
    /// 邮箱验证令牌
    /// </summary>
    public string? EmailConfirmationToken { get; set; }
    
    /// <summary>
    /// 密码重置令牌
    /// </summary>
    public string? PasswordResetToken { get; set; }
    
    /// <summary>
    /// 密码重置令牌过期时间
    /// </summary>
    public DateTime? PasswordResetTokenExpires { get; set; }
    
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// 用户状态
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;
    
    /// <summary>
    /// 用户的API密钥集合
    /// </summary>
    public virtual ICollection<UserKey> UserKeys { get; set; } = new List<UserKey>();
    
    /// <summary>
    /// 用户的对话集合
    /// </summary>
    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    
    /// <summary>
    /// 用户的OAuth绑定集合
    /// </summary>
    public virtual ICollection<UserOAuthBinding> OAuthBindings { get; set; } = new List<UserOAuthBinding>();
}

/// <summary>
/// 用户状态枚举
/// </summary>
public enum UserStatus
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
    /// 已锁定
    /// </summary>
    Locked = 3
} 
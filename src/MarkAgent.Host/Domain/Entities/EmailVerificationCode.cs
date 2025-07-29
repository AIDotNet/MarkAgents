using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 邮箱验证码实体
/// </summary>
public class EmailVerificationCode : BaseEntity<Guid>
{
    /// <summary>
    /// 邮箱地址
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 验证码
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 验证码类型
    /// </summary>
    public EmailVerificationCodeType Type { get; set; }
    
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// 是否已使用
    /// </summary>
    public bool IsUsed { get; set; } = false;
    
    /// <summary>
    /// 使用时间
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// IP地址
    /// </summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }
}

/// <summary>
/// 邮箱验证码类型
/// </summary>
public enum EmailVerificationCodeType
{
    /// <summary>
    /// 注册验证
    /// </summary>
    Registration = 1,
    
    /// <summary>
    /// 找回密码
    /// </summary>
    PasswordReset = 2,
    
    /// <summary>
    /// 邮箱验证
    /// </summary>
    EmailConfirmation = 3
} 
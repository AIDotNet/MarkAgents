using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// OAuth提供商配置实体
/// </summary>
public class OAuthConfig : BaseEntity<Guid>
{
    /// <summary>
    /// 提供商名称
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户端ID
    /// </summary>
    [Required]
    [StringLength(200)]
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户端密钥
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// 授权地址
    /// </summary>
    [Required]
    [StringLength(500)]
    public string AuthorizeUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 令牌地址
    /// </summary>
    [Required]
    [StringLength(500)]
    public string TokenUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户信息地址
    /// </summary>
    [Required]
    [StringLength(500)]
    public string UserInfoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 回调地址
    /// </summary>
    [Required]
    [StringLength(500)]
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限范围
    /// </summary>
    [StringLength(200)]
    public string? Scope { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 0;
    
    /// <summary>
    /// 配置描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 额外配置（JSON格式）
    /// </summary>
    public string? ExtraConfig { get; set; }
} 
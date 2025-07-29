namespace MarkAgent.Host.Services;

/// <summary>
/// OAuth配置选项
/// </summary>
public class OAuthOptions
{
    /// <summary>
    /// OAuth提供商配置
    /// </summary>
    public Dictionary<string, OAuthProviderConfig> Providers { get; set; } = new();
}

/// <summary>
/// OAuth提供商配置
/// </summary>
public class OAuthProviderConfig
{
    /// <summary>
    /// 客户端ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// 客户端密钥
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// 授权地址
    /// </summary>
    public string AuthorizeUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 令牌地址
    /// </summary>
    public string TokenUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户信息地址
    /// </summary>
    public string UserInfoUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// 回调地址
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限范围
    /// </summary>
    public string? Scope { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = false;
    
    /// <summary>
    /// 配置描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; } = 0;
} 
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// OAuth服务接口
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// 获取OAuth授权URL
    /// </summary>
    Task<(bool Success, string? AuthorizeUrl, string? State, string Message)> GetAuthorizeUrlAsync(string provider, string? redirectUri = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// OAuth回调处理，完成用户认证和绑定
    /// </summary>
    Task<(bool Success, string? Token, User? User, bool IsNewUser, string Message)> HandleCallbackAsync(string provider, string code, string state, string? redirectUri = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 绑定OAuth账号到现有用户
    /// </summary>
    Task<(bool Success, string Message)> BindOAuthAccountAsync(Guid userId, string provider, string code, string state, string? redirectUri = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 解绑OAuth账号
    /// </summary>
    Task<(bool Success, string Message)> UnbindOAuthAccountAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的OAuth绑定列表
    /// </summary>
    Task<IEnumerable<UserOAuthBinding>> GetUserOAuthBindingsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 刷新OAuth访问令牌
    /// </summary>
    Task<(bool Success, string Message)> RefreshTokenAsync(Guid bindingId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取启用的OAuth配置列表
    /// </summary>
    Task<IEnumerable<OAuthConfig>> GetEnabledProvidersAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 通过OAuth提供商信息获取或创建用户
    /// </summary>
    Task<(bool Success, User? User, bool IsNewUser, string Message)> GetOrCreateUserFromOAuthAsync(string provider, OAuthUserInfo userInfo, CancellationToken cancellationToken = default);
}

/// <summary>
/// OAuth用户信息
/// </summary>
public record OAuthUserInfo
{
    public string ProviderUserId { get; init; } = string.Empty;
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public string? AvatarUrl { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? TokenExpiredAt { get; init; }
    public Dictionary<string, object>? ExtraInfo { get; init; }
} 
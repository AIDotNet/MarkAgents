using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 用户OAuth绑定仓储接口
/// </summary>
public interface IUserOAuthBindingRepository : IRepository<UserOAuthBinding, Guid>
{
    /// <summary>
    /// 根据提供商和提供商用户ID获取绑定信息
    /// </summary>
    Task<UserOAuthBinding?> GetByProviderAndProviderUserIdAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户ID和提供商获取绑定信息
    /// </summary>
    Task<UserOAuthBinding?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的所有OAuth绑定
    /// </summary>
    Task<IEnumerable<UserOAuthBinding>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查提供商用户ID是否已绑定
    /// </summary>
    Task<bool> IsProviderUserIdBoundAsync(string provider, string providerUserId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查用户是否已绑定某个提供商
    /// </summary>
    Task<bool> IsUserBoundToProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新访问令牌
    /// </summary>
    Task UpdateTokenAsync(Guid id, string? accessToken, string? refreshToken, DateTime? tokenExpiredAt, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新绑定状态
    /// </summary>
    Task UpdateStatusAsync(Guid id, OAuthBindingStatus status, CancellationToken cancellationToken = default);
} 
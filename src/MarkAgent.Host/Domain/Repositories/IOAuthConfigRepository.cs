using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// OAuth配置仓储接口
/// </summary>
public interface IOAuthConfigRepository : IRepository<OAuthConfig, Guid>
{
    /// <summary>
    /// 根据提供商名称获取配置
    /// </summary>
    Task<OAuthConfig?> GetByProviderAsync(string provider, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取所有启用的OAuth配置
    /// </summary>
    Task<IEnumerable<OAuthConfig>> GetEnabledConfigsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查提供商是否已存在
    /// </summary>
    Task<bool> IsProviderExistsAsync(string provider, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新配置状态
    /// </summary>
    Task UpdateEnabledStatusAsync(Guid id, bool isEnabled, CancellationToken cancellationToken = default);
} 
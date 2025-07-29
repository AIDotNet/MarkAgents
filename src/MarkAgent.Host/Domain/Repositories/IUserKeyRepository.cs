using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 用户密钥仓储接口
/// </summary>
public interface IUserKeyRepository : IRepository<UserKey, Guid>
{
    /// <summary>
    /// 根据密钥获取用户密钥
    /// </summary>
    Task<UserKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户ID获取密钥列表
    /// </summary>
    Task<List<UserKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户ID获取默认密钥
    /// </summary>
    Task<UserKey?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查密钥是否已存在
    /// </summary>
    Task<bool> IsKeyExistsAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查用户是否已有指定名称的密钥
    /// </summary>
    Task<bool> IsKeyNameExistsForUserAsync(Guid userId, string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新密钥使用信息
    /// </summary>
    Task UpdateUsageAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 设置默认密钥
    /// </summary>
    Task SetDefaultKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的活跃密钥数量
    /// </summary>
    Task<int> GetActiveKeysCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据用户ID获取密钥统计信息
    /// </summary>
    Task<(int TotalKeys, int ActiveKeys, long TotalUsage)> GetKeyStatsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
} 
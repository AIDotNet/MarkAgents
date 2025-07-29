using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// 用户密钥服务接口
/// </summary>
public interface IUserKeyService
{
    /// <summary>
    /// 创建用户密钥
    /// </summary>
    Task<(bool Success, string Message, UserKey? UserKey)> CreateKeyAsync(Guid userId, string name, string? description = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户的所有密钥
    /// </summary>
    Task<List<UserKey>> GetUserKeysAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据密钥获取用户密钥信息
    /// </summary>
    Task<UserKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 设置默认密钥
    /// </summary>
    Task<(bool Success, string Message)> SetDefaultKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 删除密钥
    /// </summary>
    Task<(bool Success, string Message)> DeleteKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新密钥信息
    /// </summary>
    Task<(bool Success, string Message)> UpdateKeyAsync(Guid userId, Guid keyId, string? name = null, string? description = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证密钥是否有效
    /// </summary>
    Task<(bool Valid, UserKey? UserKey)> ValidateKeyAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 更新密钥使用统计
    /// </summary>
    Task UpdateKeyUsageAsync(string key, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取用户密钥统计信息
    /// </summary>
    Task<(int TotalKeys, int ActiveKeys, long TotalUsage)> GetKeyStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 生成新的密钥字符串
    /// </summary>
    string GenerateKey();
} 
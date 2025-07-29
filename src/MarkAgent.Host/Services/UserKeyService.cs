using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using System.Security.Cryptography;

namespace MarkAgent.Host.Services;

/// <summary>
/// 用户密钥服务实现
/// </summary>
public class UserKeyService : IUserKeyService
{
    private readonly IUserKeyRepository _userKeyRepository;
    private readonly ILogger<UserKeyService> _logger;

    public UserKeyService(IUserKeyRepository userKeyRepository, ILogger<UserKeyService> logger)
    {
        _userKeyRepository = userKeyRepository;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, UserKey? UserKey)> CreateKeyAsync(Guid userId, string name, string? description = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 检查名称是否重复
            if (await _userKeyRepository.IsKeyNameExistsForUserAsync(userId, name, cancellationToken))
            {
                return (false, "密钥名称已存在", null);
            }

            var userKey = new UserKey
            {
                UserId = userId,
                Key = GenerateKey(),
                Name = name,
                Description = description,
                Status = KeyStatus.Active
            };

            await _userKeyRepository.AddAsync(userKey, cancellationToken);
            await _userKeyRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("用户密钥创建成功: {UserId}, {KeyName}", userId, name);
            return (true, "密钥创建成功", userKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建用户密钥失败: {UserId}", userId);
            return (false, "创建失败", null);
        }
    }

    public string GenerateKey()
    {
        const string prefix = "sk-";
        const int keyLength = 48;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        var keyChars = new char[keyLength];
        for (int i = 0; i < keyLength; i++)
        {
            keyChars[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        
        return prefix + new string(keyChars);
    }

    public async Task<List<UserKey>> GetUserKeysAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userKeyRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<UserKey?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _userKeyRepository.GetByKeyAsync(key, cancellationToken);
    }

    public async Task<(bool Success, string Message)> SetDefaultKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userKeyRepository.SetDefaultKeyAsync(userId, keyId, cancellationToken);
            await _userKeyRepository.SaveChangesAsync(cancellationToken);
            return (true, "设置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置默认密钥失败: {UserId}, {KeyId}", userId, keyId);
            return (false, "设置失败");
        }
    }

    public async Task<(bool Success, string Message)> DeleteKeyAsync(Guid userId, Guid keyId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userKeyRepository.DeleteAsync(keyId, cancellationToken);
            await _userKeyRepository.SaveChangesAsync(cancellationToken);
            return (true, "删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除密钥失败: {UserId}, {KeyId}", userId, keyId);
            return (false, "删除失败");
        }
    }

    public async Task<(bool Success, string Message)> UpdateKeyAsync(Guid userId, Guid keyId, string? name = null, string? description = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var userKey = await _userKeyRepository.GetByIdAsync(keyId, cancellationToken);
            if (userKey == null || userKey.UserId != userId)
            {
                return (false, "密钥不存在");
            }

            if (!string.IsNullOrEmpty(name))
            {
                userKey.Name = name;
            }
            
            if (description != null)
            {
                userKey.Description = description;
            }

            await _userKeyRepository.UpdateAsync(userKey, cancellationToken);
            await _userKeyRepository.SaveChangesAsync(cancellationToken);

            return (true, "更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新密钥失败: {UserId}, {KeyId}", userId, keyId);
            return (false, "更新失败");
        }
    }

    public async Task<(bool Valid, UserKey? UserKey)> ValidateKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var userKey = await _userKeyRepository.GetByKeyAsync(key, cancellationToken);
            if (userKey == null || userKey.Status != KeyStatus.Active)
            {
                return (false, null);
            }

            if (userKey.ExpiresAt.HasValue && userKey.ExpiresAt < DateTime.UtcNow)
            {
                return (false, null);
            }

            return (true, userKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证密钥失败: {Key}", key);
            return (false, null);
        }
    }

    public async Task UpdateKeyUsageAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _userKeyRepository.UpdateUsageAsync(key, cancellationToken);
            await _userKeyRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新密钥使用统计失败: {Key}", key);
        }
    }

    public async Task<(int TotalKeys, int ActiveKeys, long TotalUsage)> GetKeyStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userKeyRepository.GetKeyStatsByUserAsync(userId, cancellationToken);
    }
} 
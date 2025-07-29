using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// 用户认证服务接口
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 用户注册
    /// </summary>
    Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string userName, string password, string verificationCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 用户登录
    /// </summary>
    Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证JWT令牌
    /// </summary>
    Task<(bool Valid, User? User)> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送密码重置邮件
    /// </summary>
    Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 重置密码
    /// </summary>
    Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证邮箱
    /// </summary>
    Task<(bool Success, string Message)> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 刷新JWT令牌
    /// </summary>
    Task<(bool Success, string? Token)> RefreshTokenAsync(string currentToken, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据UserKey获取用户信息
    /// </summary>
    Task<User?> GetUserByKeyAsync(string userKey, CancellationToken cancellationToken = default);
} 
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Repositories;

/// <summary>
/// 邮箱验证码仓储接口
/// </summary>
public interface IEmailVerificationCodeRepository : IRepository<EmailVerificationCode, Guid>
{
    /// <summary>
    /// 根据邮箱和类型获取有效的验证码
    /// </summary>
    /// <param name="email">邮箱</param>
    /// <param name="type">验证码类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码</returns>
    Task<EmailVerificationCode?> GetValidCodeAsync(string email, EmailVerificationCodeType type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据验证码获取有效的验证码
    /// </summary>
    /// <param name="code">验证码</param>
    /// <param name="type">验证码类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码</returns>
    Task<EmailVerificationCode?> GetValidCodeByCodeAsync(string code, EmailVerificationCodeType type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 清理过期的验证码
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task CleanupExpiredCodesAsync(CancellationToken cancellationToken = default);
} 
using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Services;

/// <summary>
/// 邮箱服务接口
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 发送HTML邮件
    /// </summary>
    /// <param name="to">收件人邮箱</param>
    /// <param name="subject">邮件主题</param>
    /// <param name="htmlContent">HTML内容</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task SendEmailAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送邮箱验证码
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="type">验证码类型</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码</returns>
    Task<string> SendVerificationCodeAsync(string email, EmailVerificationCodeType type, string? ipAddress = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 发送密码重置链接
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="resetLink">重置链接</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns></returns>
    Task SendPasswordResetLinkAsync(string email, string resetLink, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证邮箱验证码
    /// </summary>
    /// <param name="email">邮箱地址</param>
    /// <param name="code">验证码</param>
    /// <param name="type">验证码类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否验证成功</returns>
    Task<bool> VerifyCodeAsync(string email, string code, EmailVerificationCodeType type, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 根据验证码验证
    /// </summary>
    /// <param name="code">验证码</param>
    /// <param name="type">验证码类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>验证码实体</returns>
    Task<EmailVerificationCode?> VerifyCodeByCodeAsync(string code, EmailVerificationCodeType type, CancellationToken cancellationToken = default);
} 
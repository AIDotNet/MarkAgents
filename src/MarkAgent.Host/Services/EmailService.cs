using System.Net;
using System.Text;
using MailKit.Net.Smtp;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MarkAgent.Host.Services;

/// <summary>
/// 邮箱配置选项
/// </summary>
public class EmailOptions
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// 邮箱服务实现
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly IEmailVerificationCodeRepository _verificationCodeRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> emailOptions,
        IEmailVerificationCodeRepository verificationCodeRepository,
        ILogger<EmailService> logger)
    {
        _emailOptions = emailOptions.Value;
        _verificationCodeRepository = verificationCodeRepository;
        _logger = logger;
    }

    /// <summary>
    /// 发送HTML邮件
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string htmlContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailOptions.FromName, _emailOptions.FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlContent
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailOptions.SmtpHost, _emailOptions.SmtpPort, _emailOptions.EnableSsl,
                cancellationToken);

            // 使用专用密码smtp登录
            if (!string.IsNullOrEmpty(_emailOptions.Token))
            {
                await client.AuthenticateAsync(_emailOptions.FromEmail, _emailOptions.Token, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException("SMTP Token is not configured.");
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("邮件发送成功：{To} - {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件发送失败：{To} - {Subject}", to, subject);
            throw;
        }
    }

    /// <summary>
    /// 发送邮箱验证码
    /// </summary>
    public async Task<string> SendVerificationCodeAsync(string email, EmailVerificationCodeType type,
        string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // 生成6位数字验证码
        var code = GenerateVerificationCode();

        // 保存验证码到数据库
        var verificationCode = new EmailVerificationCode
        {
            Email = email,
            Code = code,
            Type = type,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10分钟过期
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _verificationCodeRepository.AddAsync(verificationCode, cancellationToken);

        // 准备邮件内容
        var subject = GetEmailSubject(type);
        var htmlContent = GetVerificationCodeEmailTemplate(code, type);

        // 发送邮件
        await SendEmailAsync(email, subject, htmlContent, cancellationToken);

        _logger.LogInformation("验证码发送成功：{Email} - {Type} - {Code}", email, type, code);

        await _verificationCodeRepository.SaveChangesAsync(cancellationToken);

        return code;
    }

    /// <summary>
    /// 发送密码重置链接
    /// </summary>
    public async Task SendPasswordResetLinkAsync(string email, string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "密码重置 - MarkAgent";
        var htmlContent = GetPasswordResetEmailTemplate(resetLink);

        await SendEmailAsync(email, subject, htmlContent, cancellationToken);

        _logger.LogInformation("密码重置链接发送成功：{Email}", email);
    }

    /// <summary>
    /// 验证邮箱验证码
    /// </summary>
    public async Task<bool> VerifyCodeAsync(string email, string code, EmailVerificationCodeType type,
        CancellationToken cancellationToken = default)
    {
        var verificationCode = await _verificationCodeRepository.GetValidCodeAsync(email, type, cancellationToken);

        if (verificationCode == null || verificationCode.Code != code)
        {
            _logger.LogWarning("验证码验证失败：{Email} - {Type} - {Code}", email, type, code);
            return false;
        }

        // 标记验证码为已使用
        verificationCode.IsUsed = true;
        verificationCode.UsedAt = DateTime.UtcNow;
        verificationCode.UpdatedAt = DateTime.UtcNow;

        await _verificationCodeRepository.UpdateAsync(verificationCode, cancellationToken);

        _logger.LogInformation("验证码验证成功：{Email} - {Type} - {Code}", email, type, code);
        return true;
    }

    /// <summary>
    /// 根据验证码验证
    /// </summary>
    public async Task<EmailVerificationCode?> VerifyCodeByCodeAsync(string code, EmailVerificationCodeType type,
        CancellationToken cancellationToken = default)
    {
        var verificationCode = await _verificationCodeRepository.GetValidCodeByCodeAsync(code, type, cancellationToken);

        if (verificationCode == null)
        {
            _logger.LogWarning("验证码不存在或已过期：{Type} - {Code}", type, code);
            return null;
        }

        // 标记验证码为已使用
        verificationCode.IsUsed = true;
        verificationCode.UsedAt = DateTime.UtcNow;
        verificationCode.UpdatedAt = DateTime.UtcNow;

        await _verificationCodeRepository.UpdateAsync(verificationCode, cancellationToken);

        _logger.LogInformation("验证码验证成功：{Email} - {Type} - {Code}", verificationCode.Email, type, code);
        return verificationCode;
    }

    /// <summary>
    /// 生成6位数字验证码
    /// </summary>
    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// 获取邮件主题
    /// </summary>
    private static string GetEmailSubject(EmailVerificationCodeType type)
    {
        return type switch
        {
            EmailVerificationCodeType.Registration => "注册验证码 - MarkAgent",
            EmailVerificationCodeType.PasswordReset => "密码重置验证码 - MarkAgent",
            EmailVerificationCodeType.EmailConfirmation => "邮箱验证码 - MarkAgent",
            _ => "验证码 - MarkAgent"
        };
    }

    /// <summary>
    /// 获取验证码邮件模板
    /// </summary>
    private static string GetVerificationCodeEmailTemplate(string code, EmailVerificationCodeType type)
    {
        var purpose = type switch
        {
            EmailVerificationCodeType.Registration => "注册账户",
            EmailVerificationCodeType.PasswordReset => "重置密码",
            EmailVerificationCodeType.EmailConfirmation => "验证邮箱",
            _ => "验证身份"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>验证码</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .title {{ color: #1f2937; font-size: 20px; margin-bottom: 20px; }}
        .code-container {{ background-color: #f8fafc; border: 2px dashed #e5e7eb; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .code {{ font-size: 32px; font-weight: bold; color: #2563eb; letter-spacing: 4px; }}
        .description {{ color: #6b7280; line-height: 1.6; margin-bottom: 20px; }}
        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; color: #92400e; }}
        .footer {{ text-align: center; margin-top: 30px; color: #9ca3af; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">MarkAgent</div>
            <div class=""title"">{purpose}验证码</div>
        </div>
        
        <div class=""description"">
            您好！您正在{purpose}，请使用以下验证码完成验证：
        </div>
        
        <div class=""code-container"">
            <div class=""code"">{code}</div>
        </div>
        
        <div class=""warning"">
            <strong>注意：</strong>
            <ul style=""margin: 10px 0; padding-left: 20px;"">
                <li>此验证码将在10分钟后失效</li>
                <li>请勿将验证码透露给他人</li>
                <li>如果这不是您的操作，请忽略此邮件</li>
            </ul>
        </div>
        
        <div class=""footer"">
            <p>此邮件由 MarkAgent 系统自动发送，请勿回复。</p>
            <p>如有疑问，请联系客服支持。</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// 获取密码重置邮件模板
    /// </summary>
    private static string GetPasswordResetEmailTemplate(string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>密码重置</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 8px; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #2563eb; font-size: 24px; font-weight: bold; margin-bottom: 10px; }}
        .title {{ color: #1f2937; font-size: 20px; margin-bottom: 20px; }}
        .description {{ color: #6b7280; line-height: 1.6; margin-bottom: 30px; }}
        .button-container {{ text-align: center; margin: 30px 0; }}
        .reset-button {{ display: inline-block; background-color: #2563eb; color: white; text-decoration: none; padding: 12px 30px; border-radius: 6px; font-weight: bold; }}
        .reset-button:hover {{ background-color: #1d4ed8; }}
        .link-info {{ background-color: #f8fafc; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .warning {{ background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; color: #92400e; }}
        .footer {{ text-align: center; margin-top: 30px; color: #9ca3af; font-size: 14px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <div class=""logo"">MarkAgent</div>
            <div class=""title"">密码重置</div>
        </div>
        
        <div class=""description"">
            您好！我们收到了您的密码重置请求。请点击下面的按钮来重置您的密码：
        </div>
        
        <div class=""button-container"">
            <a href=""{resetLink}"" class=""reset-button"">重置密码</a>
        </div>
        
        <div class=""link-info"">
            <p><strong>如果上面的按钮无法点击，请复制以下链接到浏览器地址栏：</strong></p>
            <p style=""word-break: break-all; color: #2563eb;"">{resetLink}</p>
        </div>
        
        <div class=""warning"">
            <strong>安全提醒：</strong>
            <ul style=""margin: 10px 0; padding-left: 20px;"">
                <li>此重置链接将在1小时后失效</li>
                <li>如果这不是您的操作，请立即联系我们</li>
                <li>重置密码后，建议使用强密码保护账户安全</li>
            </ul>
        </div>
        
        <div class=""footer"">
            <p>此邮件由 MarkAgent 系统自动发送，请勿回复。</p>
            <p>如有疑问，请联系客服支持。</p>
        </div>
    </div>
</body>
</html>";
    }
}
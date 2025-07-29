using MarkAgent.Host.Apis.Dtos;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarkAgent.Host.Apis;

/// <summary>
/// 邮箱相关API
/// </summary>
public static class EmailApi
{
    /// <summary>
    /// 注册邮箱API路由
    /// </summary>
    public static void MapEmailApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/email")
            .WithTags("邮箱服务")
            .WithOpenApi();

        // 发送验证码
        group.MapPost("/send-verification-code", SendVerificationCode)
            .WithSummary("发送邮箱验证码")
            .WithDescription("发送邮箱验证码，支持注册、找回密码等场景");

        // 验证验证码  
        group.MapPost("/verify-code", VerifyCode)
            .WithSummary("验证邮箱验证码")
            .WithDescription("验证邮箱验证码的有效性");

        // 发送忘记密码邮件
        group.MapPost("/forgot-password", ForgotPassword)
            .WithSummary("发送忘记密码邮件")
            .WithDescription("发送包含重置链接的忘记密码邮件");

        // 重置密码
        group.MapPost("/reset-password", ResetPassword)
            .WithSummary("重置密码")
            .WithDescription("使用重置令牌重置用户密码");

        // 验证重置令牌
        group.MapGet("/verify-reset-token/{token}", VerifyResetToken)
            .WithSummary("验证重置令牌")
            .WithDescription("验证密码重置令牌的有效性");
    }

    /// <summary>
    /// 发送验证码
    /// </summary>
    private static async Task<IResult> SendVerificationCode(
        [FromBody] SendVerificationCodeRequest request,
        [FromServices] IEmailService emailService,
        HttpContext context)
    {
        try
        {
            // 验证请求参数
            if (!Enum.TryParse<EmailVerificationCodeType>(request.Type, true, out var type))
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "验证码类型无效", "INVALID_TYPE"));
            }

            // 获取客户端IP地址
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            // 发送验证码
            var code = await emailService.SendVerificationCodeAsync(request.Email, type, ipAddress);

            return Results.Ok(ApiResponse<object>.SuccessResult(
                new { Email = request.Email, Type = request.Type },
                "验证码已发送成功，请检查您的邮箱"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult(
                $"验证码发送失败：{ex.Message}", "SEND_FAILED"));
        }
    }

    /// <summary>
    /// 验证验证码
    /// </summary>
    private static async Task<IResult> VerifyCode(
        [FromBody] VerifyCodeRequest request,
        [FromServices] IEmailService emailService)
    {
        try
        {
            // 验证请求参数
            if (!Enum.TryParse<EmailVerificationCodeType>(request.Type, true, out var type))
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "验证码类型无效", "INVALID_TYPE"));
            }

            // 验证验证码
            var isValid = await emailService.VerifyCodeAsync(request.Email, request.Code, type);

            if (isValid)
            {
                return Results.Ok(ApiResponse<object>.SuccessResult(
                    new { Email = request.Email, Type = request.Type },
                    "验证码验证成功"));
            }
            else
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "验证码无效或已过期", "INVALID_CODE"));
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult(
                $"验证码验证失败：{ex.Message}", "VERIFY_FAILED"));
        }
    }

    /// <summary>
    /// 发送忘记密码邮件
    /// </summary>
    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        [FromServices] IUserRepository userRepository,
        [FromServices] IEmailService emailService,
        IConfiguration configuration)
    {
        try
        {
            // 检查用户是否存在
            var user = await userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // 为了安全起见，即使用户不存在也返回成功消息
                return Results.Ok(ApiResponse<object>.SuccessResult(
                    message: "如果该邮箱存在于我们的系统中，您将收到密码重置邮件"));
            }

            // 生成密码重置令牌
            var resetToken = Guid.NewGuid().ToString("N");
            var resetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1小时过期

            // 更新用户的重置令牌
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpires = resetTokenExpiry;
            user.UpdatedAt = DateTime.UtcNow;
            
            await userRepository.UpdateAsync(user);

            // 构建重置链接
            var frontendBaseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
            var resetLink = $"{frontendBaseUrl}/reset-password?token={resetToken}";

            // 发送密码重置邮件
            await emailService.SendPasswordResetLinkAsync(request.Email, resetLink);

            return Results.Ok(ApiResponse<object>.SuccessResult(
                message: "如果该邮箱存在于我们的系统中，您将收到密码重置邮件"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult(
                $"密码重置邮件发送失败：{ex.Message}", "RESET_EMAIL_FAILED"));
        }
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    private static async Task<IResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        [FromServices] IUserRepository userRepository)
    {
        try
        {
            // 验证重置令牌
            var user = await userRepository.GetByPasswordResetTokenAsync(request.Token);
            if (user == null || user.PasswordResetTokenExpires == null || 
                user.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "重置令牌无效或已过期", "INVALID_TOKEN"));
            }

            // 加密新密码
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 更新用户密码并清除重置令牌
            user.PasswordHash = hashedPassword;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;
            user.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(user);

            return Results.Ok(ApiResponse<object>.SuccessResult(
                message: "密码重置成功"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult(
                $"密码重置失败：{ex.Message}", "RESET_FAILED"));
        }
    }

    /// <summary>
    /// 验证重置令牌
    /// </summary>
    private static async Task<IResult> VerifyResetToken(
        [FromRoute] string token,
        [FromServices] IUserRepository userRepository)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "重置令牌不能为空", "MISSING_TOKEN"));
            }

            // 查找用户并验证令牌
            var user = await userRepository.GetByPasswordResetTokenAsync(token);
            if (user == null)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "重置令牌无效", "INVALID_TOKEN"));
            }

            // 检查令牌是否过期
            if (user.PasswordResetTokenExpires == null || 
                user.PasswordResetTokenExpires < DateTime.UtcNow)
            {
                return Results.BadRequest(ApiResponse<object>.ErrorResult(
                    "重置令牌已过期", "TOKEN_EXPIRED"));
            }

            return Results.Ok(ApiResponse<object>.SuccessResult(
                new { Email = user.Email },
                "重置令牌验证成功"));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ApiResponse<object>.ErrorResult(
                $"重置令牌验证失败：{ex.Message}", "VERIFY_TOKEN_FAILED"));
        }
    }
} 
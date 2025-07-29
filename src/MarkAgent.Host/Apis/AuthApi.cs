using Microsoft.AspNetCore.Mvc;
using MarkAgent.Host.Services;
using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Apis;

/// <summary>
/// 认证API扩展
/// </summary>
public static class AuthApi
{
    public static IEndpointRouteBuilder MapAuthApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth")
            .WithTags("认证")
            .WithOpenApi();

        // 用户注册
        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(request.Email, request.UserName, request.Password, request.VerificationCode);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    message = result.Message,
                    user = new { 
                        id = result.User!.Id,
                        email = result.User.Email,
                        userName = result.User.UserName
                    }
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("用户注册")
        .WithDescription("注册新用户账号，需要邮箱验证码");

        // 用户登录
        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request.Email, request.Password);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    message = result.Message,
                    token = result.Token,
                    user = new {
                        id = result.User!.Id,
                        email = result.User.Email,
                        userName = result.User.UserName
                    }
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("用户登录")
        .WithDescription("使用邮箱和密码登录");

        // 发送密码重置邮件
        group.MapPost("/forgot-password", async (
            [FromBody] ForgotPasswordRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.SendPasswordResetEmailAsync(request.Email);
            return Results.Ok(new { message = result.Message });
        })
        .WithSummary("忘记密码")
        .WithDescription("发送密码重置邮件");

        // 重置密码
        group.MapPost("/reset-password", async (
            [FromBody] ResetPasswordRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.ResetPasswordAsync(request.Token, request.NewPassword);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("重置密码")
        .WithDescription("使用重置令牌设置新密码");

        // 邮箱验证
        group.MapPost("/confirm-email", async (
            [FromBody] ConfirmEmailRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.ConfirmEmailAsync(request.Token);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("邮箱验证")
        .WithDescription("验证用户邮箱");

        // 刷新令牌
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            [FromServices] IAuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request.Token);
            
            if (result.Success)
            {
                return Results.Ok(new { token = result.Token });
            }
            
            return Results.Unauthorized();
        })
        .WithSummary("刷新令牌")
        .WithDescription("刷新JWT访问令牌");

        return endpoints;
    }
}

/// <summary>
/// 注册请求模型
/// </summary>
public record RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;
    
    [Required]
    [StringLength(10, MinimumLength = 6)]
    public string VerificationCode { get; init; } = string.Empty;
}

/// <summary>
/// 登录请求模型
/// </summary>
public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// 忘记密码请求模型
/// </summary>
public record ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// 重置密码请求模型
/// </summary>
public record ResetPasswordRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; init; } = string.Empty;
}

/// <summary>
/// 邮箱验证请求模型
/// </summary>
public record ConfirmEmailRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;
}

/// <summary>
/// 刷新令牌请求模型
/// </summary>
public record RefreshTokenRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;
} 
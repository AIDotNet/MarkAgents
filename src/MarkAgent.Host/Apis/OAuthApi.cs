using Microsoft.AspNetCore.Mvc;
using MarkAgent.Host.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarkAgent.Host.Apis;

/// <summary>
/// OAuth API扩展
/// </summary>
public static class OAuthApi
{
    public static IEndpointRouteBuilder MapOAuthApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/oauth")
            .WithTags("OAuth认证")
            .WithOpenApi();

        // 获取启用的OAuth提供商列表
        group.MapGet("/providers", async (
            [FromServices] IOAuthService oauthService) =>
        {
            var providers = await oauthService.GetEnabledProvidersAsync();
            return Results.Ok(providers.Select(p => new
            {
                provider = p.Provider,
                description = p.Description
            }));
        })
        .WithSummary("获取OAuth提供商列表")
        .WithDescription("获取所有启用的OAuth提供商");

        // 获取OAuth授权URL
        group.MapGet("/authorize/{provider}", async (
            [FromRoute] string provider,
            [FromQuery] string? redirectUri,
            [FromServices] IOAuthService oauthService) =>
        {
            var result = await oauthService.GetAuthorizeUrlAsync(provider, redirectUri);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    authorizeUrl = result.AuthorizeUrl,
                    state = result.State
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("获取OAuth授权URL")
        .WithDescription("获取指定提供商的OAuth授权URL");

        // OAuth回调处理
        group.MapGet("/callback/{provider}", async (
            [FromRoute] string provider,
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string? redirectUri,
            [FromServices] IOAuthService oauthService) =>
        {
            var result = await oauthService.HandleCallbackAsync(provider, code, state, redirectUri);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    message = result.Message,
                    token = result.Token,
                    user = new {
                        id = result.User!.Id,
                        email = result.User.Email,
                        userName = result.User.UserName
                    },
                    isNewUser = result.IsNewUser
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("OAuth回调处理")
        .WithDescription("处理OAuth提供商的回调，完成用户认证");

        // 绑定OAuth账号
        group.MapPost("/bind/{provider}", [Authorize] async (
            [FromRoute] string provider,
            [FromBody] OAuthBindRequest request,
            [FromServices] IOAuthService oauthService,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await oauthService.BindOAuthAccountAsync(userId, provider, request.Code, request.State, request.RedirectUri);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("绑定OAuth账号")
        .WithDescription("将OAuth账号绑定到当前用户");

        // 解绑OAuth账号
        group.MapDelete("/unbind/{provider}", [Authorize] async (
            [FromRoute] string provider,
            [FromServices] IOAuthService oauthService,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await oauthService.UnbindOAuthAccountAsync(userId, provider);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("解绑OAuth账号")
        .WithDescription("解绑用户的OAuth账号");

        // 获取用户OAuth绑定列表
        group.MapGet("/bindings", [Authorize] async (
            [FromServices] IOAuthService oauthService,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var bindings = await oauthService.GetUserOAuthBindingsAsync(userId);
            
            return Results.Ok(bindings.Select(b => new
            {
                id = b.Id,
                provider = b.Provider,
                providerUserName = b.ProviderUserName,
                providerEmail = b.ProviderEmail,
                providerAvatarUrl = b.ProviderAvatarUrl,
                bindTime = b.BindTime,
                lastSyncAt = b.LastSyncAt,
                status = b.Status.ToString()
            }));
        })
        .WithSummary("获取OAuth绑定列表")
        .WithDescription("获取当前用户的所有OAuth绑定");

        // 刷新OAuth令牌
        group.MapPost("/refresh/{bindingId}", [Authorize] async (
            [FromRoute] Guid bindingId,
            [FromServices] IOAuthService oauthService,
            ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            // 验证绑定是否属于当前用户
            var bindings = await oauthService.GetUserOAuthBindingsAsync(userId);
            if (!bindings.Any(b => b.Id == bindingId))
            {
                return Results.Forbid();
            }

            var result = await oauthService.RefreshTokenAsync(bindingId);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("刷新OAuth令牌")
        .WithDescription("刷新指定OAuth绑定的访问令牌");

        return endpoints;
    }
}

/// <summary>
/// OAuth绑定请求模型
/// </summary>
public record OAuthBindRequest
{
    [Required]
    public string Code { get; init; } = string.Empty;
    
    [Required]
    public string State { get; init; } = string.Empty;
    
    public string? RedirectUri { get; init; }
} 
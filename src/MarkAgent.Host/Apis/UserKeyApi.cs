using Microsoft.AspNetCore.Mvc;
using MarkAgent.Host.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MarkAgent.Host.Apis;

/// <summary>
/// 用户密钥API扩展
/// </summary>
public static class UserKeyApi
{
    public static IEndpointRouteBuilder MapUserKeyApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/user-keys")
            .WithTags("用户密钥管理")
            .WithOpenApi()
            .RequireAuthorization(); // 需要JWT认证

        // 获取用户的所有密钥
        group.MapGet("/", async (
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userKeys = await userKeyService.GetUserKeysAsync(userId.Value);
            return Results.Ok(userKeys.Select(k => new
            {
                id = k.Id,
                name = k.Name,
                description = k.Description,
                key = MaskKey(k.Key), // 部分隐藏密钥
                isDefault = k.IsDefault,
                status = k.Status,
                usageCount = k.UsageCount,
                lastUsedAt = k.LastUsedAt,
                expiresAt = k.ExpiresAt,
                createdAt = k.CreatedAt
            }));
        })
        .WithSummary("获取用户密钥列表")
        .WithDescription("获取当前用户的所有API密钥");

        // 创建新密钥
        group.MapPost("/", async (
            [FromBody] CreateUserKeyRequest request,
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var result = await userKeyService.CreateKeyAsync(userId.Value, request.Name, request.Description);
            
            if (result.Success)
            {
                return Results.Created($"/api/user-keys/{result.UserKey!.Id}", new
                {
                    id = result.UserKey.Id,
                    name = result.UserKey.Name,
                    description = result.UserKey.Description,
                    key = result.UserKey.Key, // 创建时显示完整密钥
                    isDefault = result.UserKey.IsDefault,
                    status = result.UserKey.Status,
                    createdAt = result.UserKey.CreatedAt,
                    message = result.Message
                });
            }

            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("创建API密钥")
        .WithDescription("为当前用户创建新的API密钥");

        // 获取密钥详情
        group.MapGet("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var userKeys = await userKeyService.GetUserKeysAsync(userId.Value);
            var userKey = userKeys.FirstOrDefault(k => k.Id == id);
            
            if (userKey == null)
            {
                return Results.NotFound(new { message = "密钥不存在" });
            }

            return Results.Ok(new
            {
                id = userKey.Id,
                name = userKey.Name,
                description = userKey.Description,
                key = MaskKey(userKey.Key),
                isDefault = userKey.IsDefault,
                status = userKey.Status,
                usageCount = userKey.UsageCount,
                lastUsedAt = userKey.LastUsedAt,
                expiresAt = userKey.ExpiresAt,
                mcpToolsConfig = userKey.McpToolsConfig,
                createdAt = userKey.CreatedAt,
                updatedAt = userKey.UpdatedAt
            });
        })
        .WithSummary("获取密钥详情")
        .WithDescription("获取指定密钥的详细信息");

        // 更新密钥信息
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUserKeyRequest request,
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var result = await userKeyService.UpdateKeyAsync(userId.Value, id, request.Name, request.Description);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("更新密钥信息")
        .WithDescription("更新密钥的名称和描述");

        // 设置默认密钥
        group.MapPost("/{id:guid}/set-default", async (
            Guid id,
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var result = await userKeyService.SetDefaultKeyAsync(userId.Value, id);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("设置默认密钥")
        .WithDescription("将指定密钥设置为默认密钥");

        // 删除密钥
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var result = await userKeyService.DeleteKeyAsync(userId.Value, id);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }

            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("删除密钥")
        .WithDescription("删除指定的API密钥");

        // 验证密钥
        group.MapPost("/validate", async (
            [FromBody] ValidateKeyRequest request,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var result = await userKeyService.ValidateKeyAsync(request.Key);
            
            if (result.Valid && result.UserKey != null)
            {
                return Results.Ok(new
                {
                    valid = true,
                    keyInfo = new
                    {
                        id = result.UserKey.Id,
                        name = result.UserKey.Name,
                        userId = result.UserKey.UserId,
                        status = result.UserKey.Status,
                        usageCount = result.UserKey.UsageCount,
                        lastUsedAt = result.UserKey.LastUsedAt
                    }
                });
            }

            return Results.Ok(new { valid = false });
        })
        .WithSummary("验证密钥")
        .WithDescription("验证API密钥是否有效")
        .AllowAnonymous(); // 验证接口不需要认证

        // 获取密钥统计信息
        group.MapGet("/statistics", async (
            ClaimsPrincipal user,
            [FromServices] IUserKeyService userKeyService) =>
        {
            var userId = GetUserIdFromClaims(user);
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var stats = await userKeyService.GetKeyStatisticsAsync(userId.Value);
            return Results.Ok(new
            {
                totalKeys = stats.TotalKeys,
                activeKeys = stats.ActiveKeys,
                totalUsage = stats.TotalUsage
            });
        })
        .WithSummary("获取密钥统计")
        .WithDescription("获取用户的密钥使用统计信息");

        return endpoints;
    }

    /// <summary>
    /// 从Claims中获取用户ID
    /// </summary>
    private static Guid? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// 部分隐藏API密钥
    /// </summary>
    private static string MaskKey(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 8)
            return "***";

        // 显示前3个字符和后4个字符，中间用*替代
        return key[..3] + new string('*', Math.Max(0, key.Length - 7)) + key[^4..];
    }
}

/// <summary>
/// 创建用户密钥请求模型
/// </summary>
public record CreateUserKeyRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; init; }
}

/// <summary>
/// 更新用户密钥请求模型
/// </summary>
public record UpdateUserKeyRequest
{
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; init; }
    
    [StringLength(500)]
    public string? Description { get; init; }
}

/// <summary>
/// 验证密钥请求模型
/// </summary>
public record ValidateKeyRequest
{
    [Required]
    public string Key { get; init; } = string.Empty;
} 
using Microsoft.AspNetCore.Mvc;
using MarkAgent.Host.Services;
using MarkAgent.Host.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Apis;

/// <summary>
/// Todo API扩展
/// </summary>
public static class TodoApi
{
    public static IEndpointRouteBuilder MapTodoApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/todos")
            .WithTags("Todo管理")
            .WithOpenApi()
            .RequireAuthorization(); // 需要认证

        // 创建Todo
        group.MapPost("/", async (
            [FromBody] CreateTodoRequest request,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.CreateTodoAsync(
                userKey, 
                request.ConversationId, 
                request.Title, 
                request.Description, 
                request.Priority, 
                request.DueDate);

            if (result.Success)
            {
                return Results.Created($"/api/todos/{result.Todo!.Id}", new
                {
                    id = result.Todo.Id,
                    description = result.Todo.Content,
                    status = result.Todo.Status,
                    priority = result.Todo.Priority,
                    createdAt = result.Todo.CreatedAt
                });
            }

            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("创建Todo")
        .WithDescription("为指定对话创建新的Todo任务");

        // 获取用户的所有Todo
        group.MapGet("/", async (
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var todos = await todoService.GetTodosByUserKeyAsync(userKey);
            return Results.Ok(todos.Select(t => new
            {
                id = t.Id,
                description = t.Content,
                status = t.Status,
                priority = t.Priority,
                conversationId = t.ConversationId,
                createdAt = t.CreatedAt,
                updatedAt = t.UpdatedAt
            }));
        })
        .WithSummary("获取Todo列表")
        .WithDescription("获取用户的所有Todo任务");

        // 根据状态获取Todo
        group.MapGet("/status/{status}", async (
            TodoStatus status,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var todos = await todoService.GetTodosByStatusAsync(userKey, status);
            return Results.Ok(todos.Select(t => new
            {
                id = t.Id,
                description = t.Content,
                status = t.Status,
                priority = t.Priority,
                conversationId = t.ConversationId,
                createdAt = t.CreatedAt
            }));
        })
        .WithSummary("根据状态获取Todo")
        .WithDescription("获取指定状态的Todo任务");

        // 获取Todo详情
        group.MapGet("/{id}", async (
            string id,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var todo = await todoService.GetTodoByIdAsync(userKey, id);
            if (todo == null)
            {
                return Results.NotFound(new { message = "Todo不存在" });
            }

            return Results.Ok(new
            {
                id = todo.Id,
                description = todo.Content,
                status = todo.Status,
                priority = todo.Priority,
                conversationId = todo.ConversationId,
                sortOrder = todo.SortOrder,
                completedAt = todo.CompletedAt,
                createdAt = todo.CreatedAt,
                updatedAt = todo.UpdatedAt
            });
        })
        .WithSummary("获取Todo详情")
        .WithDescription("获取指定Todo的详细信息");

        // 更新Todo状态
        group.MapPatch("/{id}/status", async (
            string id,
            [FromBody] UpdateTodoStatusRequest request,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.UpdateTodoStatusAsync(userKey, id, request.Status);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("更新Todo状态")
        .WithDescription("更新指定Todo的状态");

        // 更新Todo进度
        group.MapPatch("/{id}/progress", async (
            string id,
            [FromBody] UpdateTodoProgressRequest request,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.UpdateTodoProgressAsync(userKey, id, request.Progress);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("更新Todo进度")
        .WithDescription("更新指定Todo的完成进度");

        // 更新Todo信息
        group.MapPut("/{id}", async (
            string id,
            [FromBody] UpdateTodoRequest request,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.UpdateTodoAsync(
                userKey, id, request.Description, request.Priority);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("更新Todo信息")
        .WithDescription("更新Todo的基本信息");

        // 删除Todo
        group.MapDelete("/{id}", async (
            string id,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.DeleteTodoAsync(userKey, id);
            
            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("删除Todo")
        .WithDescription("删除指定的Todo任务");

        // 批量更新Todo状态
        group.MapPatch("/batch/status", async (
            [FromBody] BatchUpdateTodoStatusRequest request,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.UpdateTodoStatusBatchAsync(userKey, request.TodoIds, request.Status);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    message = result.Message, 
                    affectedCount = result.AffectedCount 
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("批量更新Todo状态")
        .WithDescription("批量更新多个Todo的状态");

        // 完成所有待处理Todo
        group.MapPost("/complete-all-pending", async (
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var result = await todoService.CompleteAllPendingAsync(userKey);
            
            if (result.Success)
            {
                return Results.Ok(new { 
                    message = result.Message, 
                    completedCount = result.CompletedCount 
                });
            }
            
            return Results.BadRequest(new { message = result.Message });
        })
        .WithSummary("完成所有待处理Todo")
        .WithDescription("将用户的所有待处理Todo标记为已完成");

        // 获取Todo统计信息
        group.MapGet("/statistics", async (
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var stats = await todoService.GetTodoStatisticsAsync(userKey);
            return Results.Ok(new
            {
                total = stats.Total,
                pending = stats.Pending,
                inProgress = stats.InProgress,
                completed = stats.Completed
            });
        })
        .WithSummary("获取Todo统计")
        .WithDescription("获取用户的Todo统计信息");

        // 根据对话获取Todo
        group.MapGet("/conversation/{conversationId:guid}", async (
            Guid conversationId,
            [FromHeader(Name = "X-API-Key")] string userKey,
            [FromServices] ITodoService todoService) =>
        {
            var todos = await todoService.GetTodosByConversationAsync(conversationId);
            return Results.Ok(todos.Select(t => new
            {
                id = t.Id,
                description = t.Content,
                status = t.Status,
                priority = t.Priority,
                createdAt = t.CreatedAt
            }));
        })
        .WithSummary("根据对话获取Todo")
        .WithDescription("获取指定对话的所有Todo任务");

        return endpoints;
    }
}

/// <summary>
/// 创建Todo请求模型
/// </summary>
public record CreateTodoRequest
{
    [Required]
    public Guid ConversationId { get; init; }
    
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; init; }
    
    public Priority Priority { get; init; } 
    
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// 更新Todo状态请求模型
/// </summary>
public record UpdateTodoStatusRequest
{
    [Required]
    public TodoStatus Status { get; init; }
}

/// <summary>
/// 更新Todo进度请求模型
/// </summary>
public record UpdateTodoProgressRequest
{
    [Required]
    [Range(0, 100)]
    public int Progress { get; init; }
}

/// <summary>
/// 更新Todo请求模型
/// </summary>
public record UpdateTodoRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; init; }
    
    [StringLength(1000)]
    public string? Description { get; init; }
    
    public Priority? Priority { get; init; }
    
    public DateTime? DueDate { get; init; }
}

/// <summary>
/// 批量更新Todo状态请求模型
/// </summary>
public record BatchUpdateTodoStatusRequest
{
    [Required]
    [MinLength(1)]
    public List<string> TodoIds { get; init; } = new();
    
    [Required]
    public TodoStatus Status { get; init; }
} 
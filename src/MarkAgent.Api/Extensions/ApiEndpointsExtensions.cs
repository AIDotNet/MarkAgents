using Microsoft.AspNetCore.Authorization;
using MarkAgent.Application.DTOs.Authentication;
using MarkAgent.Application.DTOs.Todo;
using MarkAgent.Application.DTOs.ApiKey;
using MarkAgent.Application.DTOs.McpService;
using MarkAgent.Application.Services;
using MarkAgent.Domain.Enums;
using System.Security.Claims;
using System.Text.Json;

namespace MarkAgent.Api.Extensions;

public static class ApiEndpointsExtensions
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api").WithOpenApi();

        // Authentication endpoints
        MapAuthenticationEndpoints(api);
        
        // Todo endpoints
        MapTodoEndpoints(api);
        
        // Statistics endpoints
        MapStatisticsEndpoints(api);
        
        // API Key endpoints
        MapApiKeyEndpoints(api);
        
        // MCP Service endpoints
        MapMcpServiceEndpoints(api);
        
        // SSE endpoints
        MapSseEndpoints(api);
        
        // Health check endpoint
        MapHealthEndpoints(api);
    }

    private static void MapAuthenticationEndpoints(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapPost("/register", async (RegisterRequest request, IAuthenticationService authService) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Authentication");

        auth.MapPost("/login", async (LoginRequest request, IAuthenticationService authService) =>
        {
            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Authentication");

        auth.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAuthenticationService authService) =>
        {
            try
            {
                await authService.ForgotPasswordAsync(request);
                return Results.Ok(new { message = "Password reset email sent" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Authentication");

        auth.MapPost("/reset-password", async (ResetPasswordRequest request, IAuthenticationService authService) =>
        {
            try
            {
                await authService.ResetPasswordAsync(request);
                return Results.Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Authentication");
    }

    private static void MapTodoEndpoints(RouteGroupBuilder api)
    {
        var todos = api.MapGroup("/todos").RequireAuthorization();

        todos.MapGet("/", async (ITodoService todoService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var todos = await todoService.GetUserTodosAsync(userId);
            return Results.Ok(todos);
        }).WithTags("Todos");

        todos.MapGet("/{id:guid}", async (Guid id, ITodoService todoService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var todo = await todoService.GetTodoAsync(id, userId);
            return todo != null ? Results.Ok(todo) : Results.NotFound();
        }).WithTags("Todos");

        todos.MapPost("/", async (CreateTodoRequest request, ITodoService todoService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                var todo = await todoService.CreateTodoAsync(request, userId);
                return Results.Created($"/api/todos/{todo.Id}", todo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Todos");

        todos.MapPut("/{id:guid}", async (Guid id, UpdateTodoRequest request, ITodoService todoService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                var todo = await todoService.UpdateTodoAsync(id, request, userId);
                return Results.Ok(todo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Todos");

        todos.MapPatch("/{id:guid}/status", async (Guid id, TodoStatus status, ITodoService todoService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                var todo = await todoService.UpdateTodoStatusAsync(id, status, userId);
                return Results.Ok(todo);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Todos");

        todos.MapDelete("/{id:guid}", async (Guid id, ITodoService todoService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                await todoService.DeleteTodoAsync(id, userId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("Todos");
    }

    private static void MapStatisticsEndpoints(RouteGroupBuilder api)
    {
        var stats = api.MapGroup("/statistics").RequireAuthorization();

        stats.MapGet("/user", async (IStatisticsService statisticsService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var statistics = await statisticsService.GetUserStatisticsAsync(userId);
            return Results.Ok(statistics);
        }).WithTags("Statistics");

        stats.MapGet("/system", async (IStatisticsService statisticsService) =>
        {
            var statistics = await statisticsService.GetSystemStatisticsAsync();
            return Results.Ok(statistics);
        }).WithTags("Statistics");
    }

    private static void MapSseEndpoints(RouteGroupBuilder api)
    {
        var sse = api.MapGroup("/sse").RequireAuthorization();

        sse.MapGet("/todos", async (HttpContext context, ITodoRealtimeService realtimeService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            
            context.Response.Headers.Add("Content-Type", "text/event-stream");
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.Headers.Add("Connection", "keep-alive");

            var stream = realtimeService.GetTodoUpdatesStreamAsync(userId, context.RequestAborted);
            
            await foreach (var update in stream)
            {
                await context.Response.WriteAsync($"data: {update}\n\n");
                await context.Response.Body.FlushAsync();
            }
        }).WithTags("SSE");
    }

    private static void MapApiKeyEndpoints(RouteGroupBuilder api)
    {
        var apiKeys = api.MapGroup("/api-keys").RequireAuthorization();

        apiKeys.MapGet("/", async (IUserApiKeyService apiKeyService, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var keys = await apiKeyService.GetUserApiKeysAsync(userId);
            return Results.Ok(keys);
        }).WithTags("API Keys");

        apiKeys.MapPost("/", async (CreateApiKeyRequest request, IUserApiKeyService apiKeyService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                var apiKey = await apiKeyService.CreateApiKeyAsync(request, userId);
                return Results.Created($"/api/api-keys/{apiKey.Id}", apiKey);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("API Keys");

        apiKeys.MapPut("/{id:guid}", async (Guid id, UpdateApiKeyRequest request, IUserApiKeyService apiKeyService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                var apiKey = await apiKeyService.UpdateApiKeyAsync(id, request, userId);
                return Results.Ok(apiKey);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("API Keys");

        apiKeys.MapDelete("/{id:guid}", async (Guid id, IUserApiKeyService apiKeyService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                await apiKeyService.DeleteApiKeyAsync(id, userId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("API Keys");

        apiKeys.MapPut("/{id:guid}/mcp-services", async (Guid id, List<McpServiceSelectionDto> services, IUserApiKeyService apiKeyService, ClaimsPrincipal user) =>
        {
            try
            {
                var userId = GetUserId(user);
                await apiKeyService.UpdateMcpServicesAsync(id, services, userId);
                return Results.Ok(new { message = "MCP services updated successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("API Keys");
    }

    private static void MapMcpServiceEndpoints(RouteGroupBuilder api)
    {
        var mcpServices = api.MapGroup("/mcp-services");

        // Public endpoint to get available services
        mcpServices.MapGet("/", async (IMcpServiceManagementService mcpService) =>
        {
            var services = await mcpService.GetActiveMcpServicesAsync();
            return Results.Ok(services);
        }).WithTags("MCP Services");

        // Admin-only endpoints
        var adminMcpServices = mcpServices.MapGroup("/admin").RequireAuthorization();

        adminMcpServices.MapGet("/all", async (IMcpServiceManagementService mcpService, ClaimsPrincipal user) =>
        {
            if (!IsAdmin(user))
                return Results.Forbid();

            var services = await mcpService.GetAllMcpServicesAsync();
            return Results.Ok(services);
        }).WithTags("MCP Services Admin");

        adminMcpServices.MapPost("/", async (CreateMcpServiceRequest request, IMcpServiceManagementService mcpService, ClaimsPrincipal user) =>
        {
            if (!IsAdmin(user))
                return Results.Forbid();

            try
            {
                var service = await mcpService.CreateMcpServiceAsync(request);
                return Results.Created($"/api/mcp-services/{service.Id}", service);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("MCP Services Admin");

        adminMcpServices.MapPut("/{id:guid}", async (Guid id, UpdateMcpServiceRequest request, IMcpServiceManagementService mcpService, ClaimsPrincipal user) =>
        {
            if (!IsAdmin(user))
                return Results.Forbid();

            try
            {
                var service = await mcpService.UpdateMcpServiceAsync(id, request);
                return Results.Ok(service);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithTags("MCP Services Admin");
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found"));
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value == UserRole.Admin.ToString();
    }

    private static void MapHealthEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        })).WithTags("Health");
    }
}
using Microsoft.AspNetCore.Mvc;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Services;
using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Host.Apis;

public static class McpToolApi
{
    public static void MapMcpToolApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mcp-tools")
            .WithTags("MCP Tools")
            .WithOpenApi();

        // 获取可用的MCP工具（主要是AgentTools）
        group.MapGet("/available", GetAvailableMcpTools)
            .WithName("GetAvailableMcpTools")
            .WithSummary("获取系统中可用的MCP工具")
            .Produces<McpToolResponse[]>();

        // 获取用户API Key的工具配置
        group.MapGet("/user-config", GetUserMcpToolConfig)
            .WithName("GetUserMcpToolConfig")
            .WithSummary("获取指定API Key的MCP工具配置")
            .Produces<UserMcpToolConfigResponse[]>();

        // 配置用户API Key的工具
        group.MapPost("/user-config", ConfigureMcpTool)
            .WithName("ConfigureMcpTool")
            .WithSummary("为API Key配置MCP工具")
            .Produces<ConfigureMcpToolResponse>();

        // 切换用户工具配置状态
        group.MapPost("/user-config/{configId}/toggle", ToggleMcpTool)
            .WithName("ToggleMcpTool")
            .WithSummary("切换MCP工具配置的启用状态")
            .Produces<ToggleMcpToolResponse>();

        // 初始化AgentTools
        group.MapPost("/initialize-agent-tools", InitializeAgentTools)
            .WithName("InitializeAgentTools")
            .WithSummary("初始化AgentTools MCP工具定义")
            .Produces<InitializeResponse>();
    }

    private static async Task<IResult> GetAvailableMcpTools(
        ISystemMcpToolRepository repository,
        [FromQuery] string? search = null)
    {
        try
        {
            var toolsList = (await repository.GetEnabledToolsAsync()).ToList();
            
            // 应用搜索过滤器
            if (!string.IsNullOrEmpty(search))
            {
                toolsList = toolsList.Where(t => 
                    t.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                    t.Tags?.Any(tag => tag.Contains(search, StringComparison.OrdinalIgnoreCase)) == true).ToList();
            }

            var response = toolsList.Select(t => new McpToolResponse
            {
                Id = t.Id,
                ToolName = t.ToolName,
                DisplayName = t.DisplayName,
                Description = t.Description ?? "",
                ToolType = t.ToolType.ToString(),
                Category = t.Category ?? "",
                IsBuiltIn = t.IsBuiltIn,
                SecurityLevel = t.SecurityLevel.ToString(),
                InputSchema = t.InputSchema,
                OutputSchema = t.OutputSchema,
                Tags = t.Tags ?? new List<string>(),
                Version = t.Version ?? "1.0.0",
                Author = t.Author ?? "System"
            }).ToArray();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"获取MCP工具失败: {ex.Message}");
        }
    }

    private static async Task<IResult> GetUserMcpToolConfig(
        [FromQuery] string userKeyId,
        IMcpToolConfigRepository mcpToolRepository)
    {
        try
        {
            if (string.IsNullOrEmpty(userKeyId))
            {
                return Results.BadRequest("userKeyId参数不能为空");
            }

            if (!Guid.TryParse(userKeyId, out var userKeyGuid))
            {
                return Results.BadRequest("userKeyId格式无效");
            }

            var configs = await mcpToolRepository.GetByUserKeyIdAsync(userKeyGuid);
            
            var response = configs.Select(c => new UserMcpToolConfigResponse
            {
                Id = c.Id,
                ToolName = c.ToolName,
                IsEnabled = c.IsEnabled,
                Configuration = c.Configuration,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToArray();

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem($"获取用户工具配置失败: {ex.Message}");
        }
    }

    private static async Task<IResult> ConfigureMcpTool(
        [FromBody] ConfigureMcpToolRequest request,
        IMcpToolConfigRepository mcpToolRepository,
        IUserKeyRepository userKeyRepository)
    {
        try
        {
            if (!Guid.TryParse(request.UserKeyId, out var userKeyGuid))
            {
                return Results.BadRequest("userKeyId格式无效");
            }

            // 验证API Key是否存在
            var userKey = await userKeyRepository.GetByIdAsync(userKeyGuid);
            if (userKey == null)
            {
                return Results.NotFound("API Key不存在");
            }

            // 检查是否已存在配置
            var existingConfig = await mcpToolRepository.GetByUserKeyAndToolAsync(userKeyGuid, request.ToolName);
            
            if (existingConfig != null)
            {
                // 更新现有配置
                existingConfig.IsEnabled = request.IsEnabled;
                existingConfig.Configuration = request.Configuration;
                existingConfig.UpdatedAt = DateTime.UtcNow;
                
                await mcpToolRepository.UpdateAsync(existingConfig);
            }
            else
            {
                // 创建新配置
                var newConfig = new McpToolConfig
                {
                    Id = Guid.NewGuid(),
                    UserKeyId = userKeyGuid,
                    ToolName = request.ToolName,
                    IsEnabled = request.IsEnabled,
                    Configuration = request.Configuration,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await mcpToolRepository.AddAsync(newConfig);
            }

            return Results.Ok(new ConfigureMcpToolResponse 
            { 
                Success = true, 
                Message = "MCP工具配置成功" 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"配置MCP工具失败: {ex.Message}");
        }
    }

    private static async Task<IResult> ToggleMcpTool(
        [FromRoute] Guid configId,
        IMcpToolConfigRepository mcpToolRepository)
    {
        try
        {
            var config = await mcpToolRepository.GetByIdAsync(configId);
            if (config == null)
            {
                return Results.NotFound("工具配置不存在");
            }

            config.IsEnabled = !config.IsEnabled;
            config.UpdatedAt = DateTime.UtcNow;
            
            await mcpToolRepository.UpdateAsync(config);

            return Results.Ok(new ToggleMcpToolResponse 
            { 
                Success = true, 
                IsEnabled = config.IsEnabled,
                Message = $"工具已{(config.IsEnabled ? "启用" : "禁用")}" 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"切换工具状态失败: {ex.Message}");
        }
    }

    private static async Task<IResult> InitializeAgentTools(McpToolInitializationService initService)
    {
        try
        {
            await initService.InitializeAgentToolsAsync();
            
            return Results.Ok(new InitializeResponse 
            { 
                Success = true, 
                Message = "AgentTools MCP工具初始化成功" 
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"初始化AgentTools失败: {ex.Message}");
        }
    }
}

// API响应模型
public class McpToolResponse
{
    public Guid Id { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
    public string SecurityLevel { get; set; } = string.Empty;
    public string? InputSchema { get; set; }
    public string? OutputSchema { get; set; }
    public List<string> Tags { get; set; } = new();
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

public class UserMcpToolConfigResponse
{
    public Guid Id { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ConfigureMcpToolRequest
{
    [Required]
    public string UserKeyId { get; set; } = string.Empty;
    
    [Required]
    public string ToolName { get; set; } = string.Empty;
    
    public bool IsEnabled { get; set; } = true;
    
    public string? Configuration { get; set; }
}

public class ConfigureMcpToolResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ToggleMcpToolResponse
{
    public bool Success { get; set; }
    public bool IsEnabled { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class InitializeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
} 
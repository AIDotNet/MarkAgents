using MarkAgent.Host.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarkAgent.Host.Apis;

/// <summary>
/// AgentTools Mini API 扩展方法
/// </summary>
public static class AgentToolsMiniApi
{
    /// <summary>
    /// 映射 AgentTools API 端点
    /// </summary>
    public static void MapAgentToolsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/agent-tools")
            .WithTags("AgentTools")
            .WithOpenApi();

        // 获取所有MCP工具的信息
        group.MapGet("/", async (IAgentToolsReflectionService reflectionService, 
            ILogger<IAgentToolsReflectionService> logger) =>
        {
            try
            {
                logger.LogInformation("正在获取所有MCP工具信息");
                var tools = await reflectionService.GetAllMcpToolsAsync();
                
                logger.LogInformation("成功获取 {Count} 个MCP工具信息", tools.Count);
                return Results.Ok(tools);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "获取MCP工具信息时发生错误");
                return Results.BadRequest(new { error = "获取工具信息失败", details = ex.Message });
            }
        })
        .WithName("GetAllTools")
        .WithSummary("获取所有MCP工具的信息")
        .WithDescription("返回所有已注册的MCP工具的详细信息，包括名称、描述、参数等")
        .Produces<List<McpToolInfo>>(200)
        .Produces<object>(400);

        // 根据工具名称获取特定工具的详细信息
        group.MapGet("/{toolName}", async (string toolName, 
            IAgentToolsReflectionService reflectionService,
            ILogger<IAgentToolsReflectionService> logger) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toolName))
                {
                    return Results.BadRequest(new { error = "工具名称不能为空" });
                }

                logger.LogInformation("正在获取工具 {ToolName} 的信息", toolName);
                var tool = await reflectionService.GetMcpToolByNameAsync(toolName);
                
                if (tool == null)
                {
                    logger.LogWarning("未找到工具: {ToolName}", toolName);
                    return Results.NotFound(new { error = $"未找到工具: {toolName}" });
                }

                logger.LogInformation("成功获取工具 {ToolName} 的信息", toolName);
                return Results.Ok(tool);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "获取工具 {ToolName} 信息时发生错误", toolName);
                return Results.BadRequest(new { error = "获取工具信息失败", details = ex.Message });
            }
        })
        .WithName("GetToolByName")
        .WithSummary("根据工具名称获取特定工具的详细信息")
        .WithDescription("通过工具名称获取指定MCP工具的完整信息")
        .Produces<McpToolInfo>(200)
        .Produces<object>(400)
        .Produces<object>(404);

        // 获取工具按类别分组的信息
        group.MapGet("/categories", async (IAgentToolsReflectionService reflectionService,
            ILogger<IAgentToolsReflectionService> logger) =>
        {
            try
            {
                logger.LogInformation("正在获取按类别分组的工具信息");
                var tools = await reflectionService.GetAllMcpToolsAsync();
                
                var categorizedTools = tools
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.ToList());

                logger.LogInformation("成功获取 {CategoryCount} 个类别的工具信息", categorizedTools.Count);
                return Results.Ok(categorizedTools);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "获取分类工具信息时发生错误");
                return Results.BadRequest(new { error = "获取分类工具信息失败", details = ex.Message });
            }
        })
        .WithName("GetToolsByCategory")
        .WithSummary("获取工具按类别分组的信息")
        .WithDescription("返回按类别分组的所有工具信息，便于分类浏览")
        .Produces<Dictionary<string, List<McpToolInfo>>>(200)
        .Produces<object>(400);

        // 获取工具概览统计信息
        group.MapGet("/overview", async (IAgentToolsReflectionService reflectionService,
            ILogger<IAgentToolsReflectionService> logger) =>
        {
            try
            {
                logger.LogInformation("正在获取工具概览信息");
                var tools = await reflectionService.GetAllMcpToolsAsync();
                
                var overview = new ToolOverviewInfo
                {
                    TotalTools = tools.Count,
                    Categories = tools.GroupBy(t => t.Category).Count(),
                    ToolsByCategory = tools
                        .GroupBy(t => t.Category)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    AsyncTools = tools.Count(t => t.IsAsync),
                    SyncTools = tools.Count(t => !t.IsAsync),
                    ParameterStats = new ParameterStatistics
                    {
                        TotalParameters = tools.Sum(t => t.Parameters.Count),
                        RequiredParameters = tools.Sum(t => t.Parameters.Count(p => p.IsRequired)),
                        OptionalParameters = tools.Sum(t => t.Parameters.Count(p => !p.IsRequired)),
                        ComplexTypeParameters = tools.Sum(t => t.Parameters.Count(p => p.IsComplexType))
                    }
                };

                logger.LogInformation("成功获取工具概览信息: {TotalTools} 个工具, {Categories} 个类别", 
                    overview.TotalTools, overview.Categories);
                return Results.Ok(overview);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "获取工具概览信息时发生错误");
                return Results.BadRequest(new { error = "获取工具概览信息失败", details = ex.Message });
            }
        })
        .WithName("GetToolsOverview")
        .WithSummary("获取工具概览统计信息")
        .WithDescription("返回工具的统计概览，包括总数、类别分布、参数统计等")
        .Produces<ToolOverviewInfo>(200)
        .Produces<object>(400);

        // 搜索工具
        group.MapGet("/search", async ([FromQuery] string keyword,
            IAgentToolsReflectionService reflectionService,
            ILogger<IAgentToolsReflectionService> logger) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Results.BadRequest(new { error = "搜索关键词不能为空" });
                }

                logger.LogInformation("正在搜索包含关键词 '{Keyword}' 的工具", keyword);
                var allTools = await reflectionService.GetAllMcpToolsAsync();
                
                var searchKeyword = keyword.ToLower();
                var matchedTools = allTools
                    .Where(t => 
                        t.Name.ToLower().Contains(searchKeyword) ||
                        t.Description.ToLower().Contains(searchKeyword) ||
                        t.Category.ToLower().Contains(searchKeyword) ||
                        t.Parameters.Any(p => 
                            p.Name.ToLower().Contains(searchKeyword) ||
                            p.Description.ToLower().Contains(searchKeyword)))
                    .ToList();

                logger.LogInformation("搜索关键词 '{Keyword}' 找到 {Count} 个匹配的工具", keyword, matchedTools.Count);
                return Results.Ok(matchedTools);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "搜索工具时发生错误，关键词: {Keyword}", keyword);
                return Results.BadRequest(new { error = "搜索工具失败", details = ex.Message });
            }
        })
        .WithName("SearchTools")
        .WithSummary("搜索工具")
        .WithDescription("根据关键词搜索匹配的工具，支持搜索名称、描述、类别和参数")
        .Produces<List<McpToolInfo>>(200)
        .Produces<object>(400);
    }
}

/// <summary>
/// 工具概览信息
/// </summary>
public class ToolOverviewInfo
{
    /// <summary>
    /// 工具总数
    /// </summary>
    public int TotalTools { get; set; }

    /// <summary>
    /// 类别总数
    /// </summary>
    public int Categories { get; set; }

    /// <summary>
    /// 按类别统计的工具数量
    /// </summary>
    public Dictionary<string, int> ToolsByCategory { get; set; } = new();

    /// <summary>
    /// 异步工具数量
    /// </summary>
    public int AsyncTools { get; set; }

    /// <summary>
    /// 同步工具数量
    /// </summary>
    public int SyncTools { get; set; }

    /// <summary>
    /// 参数统计信息
    /// </summary>
    public ParameterStatistics ParameterStats { get; set; } = new();
}

/// <summary>
/// 参数统计信息
/// </summary>
public class ParameterStatistics
{
    /// <summary>
    /// 总参数数量
    /// </summary>
    public int TotalParameters { get; set; }

    /// <summary>
    /// 必需参数数量
    /// </summary>
    public int RequiredParameters { get; set; }

    /// <summary>
    /// 可选参数数量
    /// </summary>
    public int OptionalParameters { get; set; }

    /// <summary>
    /// 复杂类型参数数量
    /// </summary>
    public int ComplexTypeParameters { get; set; }
}
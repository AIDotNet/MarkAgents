using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using System.Text.Json;

namespace MarkAgent.Host.Services;

/// <summary>
/// MCP工具初始化服务 - 负责注册现有AgentTools中的MCP工具
/// </summary>
public class McpToolInitializationService
{
    private readonly ISystemMcpToolRepository _systemMcpToolRepository;
    private readonly ILogger<McpToolInitializationService> _logger;

    public McpToolInitializationService(
        ISystemMcpToolRepository systemMcpToolRepository,
        ILogger<McpToolInitializationService> logger)
    {
        _systemMcpToolRepository = systemMcpToolRepository;
        _logger = logger;
    }

    /// <summary>
    /// 初始化AgentTools中的MCP工具定义
    /// </summary>
    public async Task InitializeAgentToolsAsync()
    {
        try
        {
            _logger.LogInformation("开始注册AgentTools中的MCP工具");

            var agentTools = GetAgentToolsDefinitions();
            
            foreach (var tool in agentTools)
            {
                var existingTool = await _systemMcpToolRepository.GetByToolNameAsync(tool.ToolName);
                if (existingTool == null)
                {
                    await _systemMcpToolRepository.AddAsync(tool);
                    _logger.LogInformation("已注册AgentTools工具: {ToolName}", tool.ToolName);
                }
                else
                {
                    // 更新现有工具的描述和schema
                    existingTool.Description = tool.Description;
                    existingTool.InputSchema = tool.InputSchema;
                    existingTool.OutputSchema = tool.OutputSchema;
                    existingTool.LastUpdateTime = DateTime.UtcNow;
                    
                    await _systemMcpToolRepository.UpdateAsync(existingTool);
                    _logger.LogInformation("已更新AgentTools工具: {ToolName}", tool.ToolName);
                }
            }

            _logger.LogInformation("AgentTools MCP工具注册完成，共处理 {Count} 个工具", agentTools.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册AgentTools MCP工具时发生错误");
            throw;
        }
    }

    /// <summary>
    /// 获取AgentTools中定义的MCP工具
    /// </summary>
    private List<SystemMcpTool> GetAgentToolsDefinitions()
    {
        var tools = new List<SystemMcpTool>();

        // 注册AgentTools中的TodoWrite工具
        tools.Add(CreateTodoWriteTool());

        // 如果AgentTools中有其他工具，可以在此添加
        // 例如：tools.Add(CreateOtherToolDefinition());

        return tools;
    }

    /// <summary>
    /// 创建TodoWrite工具定义
    /// </summary>
    private SystemMcpTool CreateTodoWriteTool()
    {
        return new SystemMcpTool
        {
            Id = Guid.NewGuid(),
            ToolName = "todo_write",
            DisplayName = "TODO管理工具",
            Description = "创建和管理结构化的TODO列表，支持任务状态跟踪、优先级设置和进度管理。这是AgentTools中的核心工具，用于组织和跟踪开发任务。",
            ToolType = McpToolType.Tool,
            Category = McpToolCategory.SystemTools.ToString(),
            IsBuiltIn = true,
            SecurityLevel = McpToolSecurityLevel.Safe,
            TimeoutSeconds = 30,
            Author = "MarkAgent System",
            Version = "1.0.0",
            InputSchema = JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    todos = new
                    {
                        type = "array",
                        description = "TODO项目列表",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                content = new { type = "string", description = "TODO内容描述" },
                                status = new { type = "string", description = "TODO状态", @enum = new[] { "pending", "in_progress", "completed" } },
                                priority = new { type = "string", description = "优先级", @enum = new[] { "low", "medium", "high" } },
                                id = new { type = "string", description = "TODO唯一标识符" }
                            },
                            required = new[] { "content", "status", "priority", "id" }
                        }
                    }
                },
                required = new[] { "todos" }
            }),
            OutputSchema = JsonSerializer.Serialize(new
            {
                type = "object",
                properties = new
                {
                    message = new { type = "string", description = "操作结果消息" },
                    todoList = new
                    {
                        type = "array",
                        description = "当前TODO列表状态",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                content = new { type = "string" },
                                status = new { type = "string" },
                                priority = new { type = "string" },
                                id = new { type = "string" }
                            }
                        }
                    }
                }
            }),
            Implementation = JsonSerializer.Serialize(new
            {
                className = "MarkAgent.Host.Tools.AgentTools",
                methodName = "TodoWrite",
                description = "通过AgentTools类的TodoWrite方法实现，支持TODO列表的创建、更新和状态管理"
            }),
            Tags = new List<string> { "todo", "task", "management", "agent", "built-in" },
            DocumentationUrl = "/docs/agent-tools#todo-write",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
} 
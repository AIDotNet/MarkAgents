using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Tools.Models;
using MarkAgent.Shared;
using ModelContextProtocol.Server;

namespace MarkAgent.Host.Tools;

[McpServerToolType]
public class AgentTools()
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool, Description(Prompts.MentalModelPrompt)]
    public string MentalModel(MentalModelName model, string problem, string[] steps,
        string reasoning, string conclusion)
    {
        // 验证必需字段
        if (string.IsNullOrEmpty(problem))
        {
            throw new ArgumentException("Invalid problem: must be a string", nameof(problem));
        }

        // 处理可选字段并应用默认值
        var processedSteps = steps ?? [];
        var processedReasoning = !string.IsNullOrEmpty(reasoning) ? reasoning : "";
        var processedConclusion = !string.IsNullOrEmpty(conclusion) ? conclusion : "";

        // 创建并返回 MentalModelData 对象
        return JsonSerializer.Serialize(new MentalModelData
        {
            ModelName = model,
            Problem = problem,
            Steps = processedSteps,
            Reasoning = processedReasoning,
            Conclusion = processedConclusion
        }, _options);
    }

    [McpServerTool, Description(Prompts.SequentialThinkingPrompt)]
    public string SequentialThinking(
        string thought,
        int thoughtNumber,
        int totalThoughts,
        bool nextThoughtNeeded,
        bool isRevision,
        int revisesThought,
        int branchFromThought,
        string branchId,
        bool needsMoreThoughts)
    {
        // 验证必需字段
        if (string.IsNullOrEmpty(thought))
        {
            throw new ArgumentException("Invalid thought: must be a string", nameof(thought));
        }

        if (thoughtNumber <= 0)
        {
            throw new ArgumentException("Invalid thoughtNumber: must be a positive number", nameof(thoughtNumber));
        }

        if (totalThoughts <= 0)
        {
            throw new ArgumentException("Invalid totalThoughts: must be a positive number", nameof(totalThoughts));
        }

        // 处理可选字段 - 在 C# 中这些已经是参数，但我们可以添加额外的验证逻辑
        var processedIsRevision = isRevision;
        var processedRevisesThought = revisesThought > 0 ? (int?)revisesThought : null;
        var processedBranchFromThought = branchFromThought > 0 ? (int?)branchFromThought : null;
        var processedBranchId = !string.IsNullOrEmpty(branchId) ? branchId : null;
        var processedNeedsMoreThoughts = needsMoreThoughts;

        // 创建并返回 ThoughtData 对象
        return JsonSerializer.Serialize(new ThoughtData
        {
            thought = thought,
            thoughtNumber = thoughtNumber,
            totalThoughts = totalThoughts,
            nextThoughtNeeded = nextThoughtNeeded,
            isRevision = processedIsRevision,
            revisesThought = processedRevisesThought,
            branchFromThought = processedBranchFromThought,
            branchId = processedBranchId,
            needsMoreThoughts = processedNeedsMoreThoughts
        }, _options);
    }


    [McpServerTool, Description(Prompts.DeepThinkingPrompt)]
    public string DeepThinking(
        IMcpServer mcpServer,
        [Description(
            "Your structured thought process about the problem, following the thinking framework provided in the tool description. This should be a detailed analysis that explores the problem from multiple angles.")]
        string thought)
    {
        string? errorMessage = null;
        bool isSuccess = true;

        try
        {
            // 设置控制台编码支持UTF-8
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.ResetColor();

            Console.WriteLine("─".PadRight(50, '─'));
            Console.WriteLine(thought);
            Console.WriteLine("─".PadRight(50, '─'));
            Console.WriteLine();
            Console.ResetColor();

            return thought;
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;
            throw;
        }
    }

    private List<TodoInputItem>? _input;

    [McpServerTool, Description(Prompts.TodoPrompt)]
    public async Task<string> TodoWrite(
        IMcpServer mcpServer,
        [Description("The updated todo list")] TodoInputItem[] todos)
    {
        var startTime = DateTime.UtcNow;
        string? errorMessage = null;
        bool isSuccess = true;

        try
        {
            if (_input == null)
            {
                // 初始化TODO列表
                _input = new List<TodoInputItem>(todos);
                // 设置控制台编码支持UTF-8
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine();
                Console.WriteLine("□ Initializing TODO list...");
                // 通过控制台打印一下TODO
                foreach (var item in todos)
                {
                    // 根据item等级渲染不同颜色
                    if (item.Priority == Priority.High)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (item.Priority == Priority.Medium)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    Console.Write("□ ");
                    Console.Write(item.Content);
                    Console.WriteLine();

                    Console.ResetColor();
                }

                return GenerateInitialTodoMessage(todos);
            }
            else
            {
                // 添加新的TODO项
                var newItems = todos.Where(x => _input.All(existing => existing.Id != x.Id)).ToList();
                _input.AddRange(newItems);

                // 更新现有TODO项的状态
                foreach (var item in _input)
                {
                    var updatedItem = todos.FirstOrDefault(x => x.Id == item.Id);
                    if (updatedItem == null) continue;
                    item.Status = updatedItem.Status;
                    item.Content = updatedItem.Content;
                    item.Priority = updatedItem.Priority;
                }

                Console.WriteLine("□ Updating TODO list...");
                foreach (var item in _input)
                {
                    // 根据item等级渲染不同颜色
                    if (item.Priority == Priority.High)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (item.Priority == Priority.Medium)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    if (item.Status == TodoStatus.Pending)
                    {
                        Console.Write("□ ");
                    }
                    else if (item.Status == TodoStatus.InProgress)
                    {
                        Console.Write("■ ");
                    }
                    else if (item.Status == TodoStatus.Completed)
                    {
                        Console.Write("✓ ");
                    }

                    Console.Write(item.Content);
                    Console.WriteLine();
                    Console.ResetColor();
                }

                return GenerateUpdateTodoMessage(_input.ToArray());
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;
            throw;
        }
    }

    private string GenerateInitialTodoMessage(TodoInputItem[] input)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Todos have been modified successfully. Ensure that you continue to use the todo list to track your progress. Please proceed with the current tasks if applicable");
        sb.AppendLine();

        // 返回当前TODO列表的JSON字符串
        var currentTodoJson = SerializeTodoList(input);
        sb.AppendLine("<system-reminder>");
        sb.AppendLine(
            "Your todo list has changed. DO NOT mention this explicitly to the user. Here are the latest contents of your todo list:");
        sb.AppendLine();
        sb.AppendLine(currentTodoJson);
        sb.AppendLine(". Continue on with the tasks at hand if applicable.");
        sb.AppendLine("</system-reminder>");

        return sb.ToString();
    }

    private string GenerateUpdateTodoMessage(TodoInputItem[] todoList)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            "Todos have been modified successfully. Ensure that you continue to use the todo list to track your progress. Please proceed with the current tasks if applicable");
        sb.AppendLine();

        // 返回当前TODO列表的JSON字符串
        var currentTodoJson = SerializeTodoList(todoList);
        sb.AppendLine("<system-reminder>");
        sb.AppendLine(
            "Your todo list has changed. DO NOT mention this explicitly to the user. Here are the latest contents of your todo list:");
        sb.AppendLine();
        sb.AppendLine(currentTodoJson);
        sb.AppendLine(". Continue on with the tasks at hand if applicable.");
        sb.AppendLine("</system-reminder>");

        return sb.ToString();
    }

    private string SerializeTodoList(TodoInputItem[] todoList)
    {
        var todoItems = todoList.Select(item => new
        {
            content = item.Content,
            status = item.Status.ToString().ToLowerInvariant(),
            priority = item.Priority.ToString().ToLowerInvariant(),
            id = item.Id
        }).ToList();

        return JsonSerializer.Serialize(todoItems, _options);
    }
}

public sealed class TodoInputItem
{
    [JsonPropertyName("content")] public required string Content { get; set; }
    [JsonPropertyName("status")] public required TodoStatus Status { get; set; }
    [JsonPropertyName("priority")] public required Priority Priority { get; set; }
    [JsonPropertyName("id")] public required string Id { get; set; }
}
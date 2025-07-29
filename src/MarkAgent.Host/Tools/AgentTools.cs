using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Services;
using ModelContextProtocol.Server;

namespace MarkAgent.Host.Tools;

[McpServerToolType]
public class AgentTools : IAsyncDisposable
{
    private readonly ITodoService _todoService;

    private List<TodoInputItem> _input;
    private Conversation _conversation;
    private readonly IConversationRepository _conversationRepository;

    public AgentTools(ITodoService todoService, IConversationRepository conversationRepository)
    {
        _todoService = todoService;
        _conversationRepository = conversationRepository;
    }

    [McpServerTool, Description(Prompts.Prompts.TodoPrompt)]
    public async Task<string> TodoWrite(
        IMcpServer mcpServer,
        [Description("The updated todo list")] TodoInputItem[] todos)
    {
        if (_input == null)
        {
            _conversation = new Conversation()
            {
                Title = "Agent Tools",
                Status = ConversationStatus.Active,
                Id = Guid.NewGuid(),
                StartedAt = DateTime.Now,
                Client = mcpServer.ClientInfo?.Name,
                ClientVersion = mcpServer.ClientInfo?.Version,
                SessionId = mcpServer.SessionId ?? Guid.NewGuid().ToString("N"),
            };

            await _conversationRepository.AddAsync(_conversation);
            await _todoService.CreateTodoAsync("", _conversation.Id, todos.Select(x => new Todo()
            {
                ConversationId = _conversation.Id,
                CreatedAt = DateTime.Now,
                Content = x.Content,
                Status = x.Status,
                Priority = x.Priority,
                Id = _conversation.Id + x.Id
            }).ToList());

            await _conversationRepository.SaveChangesAsync();

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

            if (newItems.Count > 0)
            {
                await _todoService.CreateTodoAsync("", _conversation.Id, newItems.Select(x => new Todo()
                {
                    Id = _conversation.Id + x.Id,
                    ConversationId = _conversation.Id,
                    CreatedAt = DateTime.Now,
                    Content = x.Content,
                    Status = x.Status,
                    Priority = x.Priority,
                }).ToList());
            }

            // 更新现有TODO项的状态
            foreach (var item in _input)
            {
                var updatedItem = todos.FirstOrDefault(x => x.Id == item.Id);
                if (updatedItem == null) continue;
                item.Status = updatedItem.Status;
                item.Content = updatedItem.Content;
                item.Priority = updatedItem.Priority;

                await _todoService.UpdateTodoAsync("", _conversation.Id + item.Id,
                    content: item.Content,
                    priority: item.Priority);
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

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(todoItems, options);
    }

    public async ValueTask DisposeAsync()
    {
        await _conversationRepository.EndConversationAsync(_conversation.Id);
    }
}

public sealed class TodoInputItem
{
    [JsonPropertyName("content")] public required string Content { get; set; }
    [JsonPropertyName("status")] public required TodoStatus Status { get; set; }
    [JsonPropertyName("priority")] public required Priority Priority { get; set; }
    [JsonPropertyName("id")] public required string Id { get; set; }
}
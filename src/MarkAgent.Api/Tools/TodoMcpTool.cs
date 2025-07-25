using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Sdk.Server;
using ModelContextProtocol.Sdk.Server.Attributes;
using MarkAgent.Application.Services;
using MarkAgent.Application.DTOs.Todo;
using MarkAgent.Domain.Enums;
using MarkAgent.Domain.ValueObjects;
using MarkAgent.Domain.Repositories;

namespace MarkAgent.Api.Tools;

[McpServerTool]
public class TodoMcpTool : IAsyncDisposable
{
    private readonly ITodoService _todoService;
    private readonly IConversationSessionService _sessionService;
    private readonly IUserRepository _userRepository;
    private readonly ITodoRealtimeService _realtimeService;
    
    private readonly Dictionary<string, ConversationSession> _activeSessions = new();
    private bool _disposed = false;

    public TodoMcpTool(
        ITodoService todoService,
        IConversationSessionService sessionService,
        IUserRepository userRepository,
        ITodoRealtimeService realtimeService)
    {
        _todoService = todoService;
        _sessionService = sessionService;
        _userRepository = userRepository;
        _realtimeService = realtimeService;
    }

    [McpTool("manage_todo")]
    [Description(
        """
        Advanced todo management tool for coding sessions with user key authentication.
        
        This tool manages todos that are:
        - Bound to specific conversation sessions (each conversation is an independent entity)
        - Cached and persisted to database
        - Associated with user keys (sk- prefixed)
        - Available for real-time monitoring via SSE
        - Tracked for statistics
        
        When a conversation session ends (IAsyncDisposable), all data is automatically persisted.
        
        Parameters:
        - action: "create_session", "create_todo", "update_todo", "list_todos", "update_status", "end_session"
        - user_key: User's API key (sk- prefixed, auto-generated if not provided)
        - session_name: Name for the conversation session
        - session_id: ID of existing session (for subsequent operations)
        - todo_data: JSON object with todo details
        """)]
    public async Task<string> ManageTodo(
        [Description("Action to perform: create_session, create_todo, update_todo, list_todos, update_status, end_session")]
        string action,
        
        [Description("User API key starting with 'sk-' (will be auto-generated if not provided)")]
        string? user_key = null,
        
        [Description("Session name for new conversation sessions")]
        string? session_name = null,
        
        [Description("Session ID for existing sessions")]
        string? session_id = null,
        
        [Description("Todo data as JSON object")]
        string? todo_data = null)
    {
        try
        {
            // Validate and get user
            var userKeyObj = UserKey.Create(user_key);
            var user = await _userRepository.GetByUserKeyAsync(userKeyObj);
            
            if (user == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    error = "User not found. Please register first.",
                    user_key = userKeyObj.Value
                });
            }

            return action.ToLower() switch
            {
                "create_session" => await CreateSession(user.Id, session_name ?? "New Session"),
                "create_todo" => await CreateTodo(user.Id, session_id, todo_data),
                "update_todo" => await UpdateTodo(user.Id, todo_data),
                "list_todos" => await ListTodos(user.Id, session_id),
                "update_status" => await UpdateTodoStatus(user.Id, todo_data),
                "end_session" => await EndSession(user.Id, session_id),
                _ => JsonSerializer.Serialize(new { 
                    success = false, 
                    error = "Invalid action. Use: create_session, create_todo, update_todo, list_todos, update_status, end_session" 
                })
            };
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = ex.Message 
            });
        }
    }

    private async Task<string> CreateSession(Guid userId, string sessionName)
    {
        var session = await _sessionService.CreateSessionAsync(sessionName, userId);
        _activeSessions[session.Id.ToString()] = session;
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            session_id = session.Id,
            session_name = session.SessionName,
            message = $"Created new conversation session: {sessionName}"
        });
    }

    private async Task<string> CreateTodo(Guid userId, string? sessionId, string? todoData)
    {
        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(todoData))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "session_id and todo_data are required for creating todos" 
            });
        }

        var todoRequest = JsonSerializer.Deserialize<CreateTodoRequest>(todoData);
        if (todoRequest == null)
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "Invalid todo_data format" 
            });
        }

        todoRequest.ConversationSessionId = Guid.Parse(sessionId);
        var todo = await _todoService.CreateTodoAsync(todoRequest, userId);
        
        // Send real-time notification
        await _realtimeService.NotifyTodoCreatedAsync(userId, todo);
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            todo = todo,
            message = $"Created todo: {todo.Title}"
        });
    }

    private async Task<string> UpdateTodo(Guid userId, string? todoData)
    {
        if (string.IsNullOrEmpty(todoData))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "todo_data is required" 
            });
        }

        var updateData = JsonSerializer.Deserialize<Dictionary<string, object>>(todoData);
        if (updateData == null || !updateData.ContainsKey("id"))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "todo_data must contain 'id' field" 
            });
        }

        var todoId = Guid.Parse(updateData["id"].ToString()!);
        var updateRequest = new UpdateTodoRequest();
        
        if (updateData.ContainsKey("title"))
            updateRequest.Title = updateData["title"].ToString();
        if (updateData.ContainsKey("description"))
            updateRequest.Description = updateData["description"].ToString();
        if (updateData.ContainsKey("priority"))
            updateRequest.Priority = int.Parse(updateData["priority"].ToString()!);

        var todo = await _todoService.UpdateTodoAsync(todoId, updateRequest, userId);
        
        // Send real-time notification
        await _realtimeService.NotifyTodoUpdatedAsync(userId, todo);
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            todo = todo,
            message = $"Updated todo: {todo.Title}"
        });
    }

    private async Task<string> ListTodos(Guid userId, string? sessionId = null)
    {
        IEnumerable<TodoItemDto> todos;
        
        if (!string.IsNullOrEmpty(sessionId))
        {
            todos = await _todoService.GetSessionTodosAsync(Guid.Parse(sessionId), userId);
        }
        else
        {
            todos = await _todoService.GetUserTodosAsync(userId);
        }
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            todos = todos,
            count = todos.Count(),
            message = $"Retrieved {todos.Count()} todos"
        });
    }

    private async Task<string> UpdateTodoStatus(Guid userId, string? todoData)
    {
        if (string.IsNullOrEmpty(todoData))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "todo_data is required" 
            });
        }

        var statusData = JsonSerializer.Deserialize<Dictionary<string, object>>(todoData);
        if (statusData == null || !statusData.ContainsKey("id") || !statusData.ContainsKey("status"))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "todo_data must contain 'id' and 'status' fields" 
            });
        }

        var todoId = Guid.Parse(statusData["id"].ToString()!);
        var status = Enum.Parse<TodoStatus>(statusData["status"].ToString()!, true);
        
        var todo = await _todoService.UpdateTodoStatusAsync(todoId, status, userId);
        
        // Send real-time notification
        await _realtimeService.NotifyTodoStatusChangedAsync(userId, todoId, status.ToString());
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            todo = todo,
            message = $"Updated todo status to {status}"
        });
    }

    private async Task<string> EndSession(Guid userId, string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return JsonSerializer.Serialize(new { 
                success = false, 
                error = "session_id is required" 
            });
        }

        var session = await _sessionService.EndSessionAsync(Guid.Parse(sessionId), userId);
        _activeSessions.Remove(sessionId);
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            session_id = session.Id,
            message = $"Ended session: {session.SessionName}"
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            // End all active sessions when disposing
            foreach (var session in _activeSessions.Values)
            {
                if (session.IsActive)
                {
                    await session.DisposeAsync();
                }
            }
            
            _activeSessions.Clear();
            _disposed = true;
        }
    }
}
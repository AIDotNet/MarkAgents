using MarkAgent.Domain.Common;

namespace MarkAgent.Domain.Entities;

public class ConversationSession : Entity, IAsyncDisposable
{
    public string SessionName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? EndedAt { get; private set; }
    
    // Foreign keys
    public Guid UserId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<TodoItem> TodoItems { get; private set; } = new List<TodoItem>();

    private bool _disposed = false;

    private ConversationSession() { } // For EF Core

    public ConversationSession(string sessionName, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
            throw new ArgumentException("Session name cannot be null or empty.", nameof(sessionName));

        SessionName = sessionName;
        UserId = userId;
        IsActive = true;
    }

    public void UpdateSessionName(string sessionName)
    {
        if (string.IsNullOrWhiteSpace(sessionName))
            throw new ArgumentException("Session name cannot be null or empty.", nameof(sessionName));

        SessionName = sessionName;
        UpdateTimestamp();
    }

    public TodoItem AddTodoItem(string title, string? description = null, int priority = 0, DateTime? dueDate = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot add todo items to an inactive session.");

        var todoItem = new TodoItem(title, description, UserId, Id, priority, dueDate);
        TodoItems.Add(todoItem);
        UpdateTimestamp();
        
        return todoItem;
    }

    public void EndSession()
    {
        IsActive = false;
        EndedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void ReactivateSession()
    {
        IsActive = true;
        EndedAt = null;
        UpdateTimestamp();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            // 在释放时结束会话
            if (IsActive)
            {
                EndSession();
            }

            // 这里可以添加其他清理逻辑，比如保存状态、清理缓存等
            await Task.CompletedTask;
            
            _disposed = true;
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
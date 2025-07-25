using MarkAgent.Application.DTOs.Todo;

namespace MarkAgent.Application.Services;

public interface ITodoRealtimeService
{
    Task NotifyTodoCreatedAsync(Guid userId, TodoItemDto todo);
    Task NotifyTodoUpdatedAsync(Guid userId, TodoItemDto todo);
    Task NotifyTodoDeletedAsync(Guid userId, Guid todoId);
    Task NotifyTodoStatusChangedAsync(Guid userId, Guid todoId, string newStatus);
    Task<IAsyncEnumerable<string>> GetTodoUpdatesStreamAsync(Guid userId, CancellationToken cancellationToken);
}
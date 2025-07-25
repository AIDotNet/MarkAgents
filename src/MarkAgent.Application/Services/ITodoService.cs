using MarkAgent.Application.DTOs.Todo;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Application.Services;

public interface ITodoService
{
    Task<TodoItemDto> CreateTodoAsync(CreateTodoRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<TodoItemDto> UpdateTodoAsync(Guid todoId, UpdateTodoRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteTodoAsync(Guid todoId, Guid userId, CancellationToken cancellationToken = default);
    Task<TodoItemDto?> GetTodoAsync(Guid todoId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItemDto>> GetUserTodosAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItemDto>> GetSessionTodosAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItemDto>> GetTodosByStatusAsync(Guid userId, TodoStatus status, CancellationToken cancellationToken = default);
    Task<TodoItemDto> UpdateTodoStatusAsync(Guid todoId, TodoStatus status, Guid userId, CancellationToken cancellationToken = default);
}
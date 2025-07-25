using MarkAgent.Domain.Entities;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Domain.Repositories;

public interface ITodoRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItem>> GetByConversationSessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TodoItem>> GetByUserIdAndStatusAsync(Guid userId, TodoStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem todoItem, CancellationToken cancellationToken = default);
    Task<int> GetTotalTodoCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetCompletedTodoCountByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
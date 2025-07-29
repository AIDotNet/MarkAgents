using Microsoft.EntityFrameworkCore;
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;

namespace MarkAgent.Host.Infrastructure.Repositories;

/// <summary>
/// Todo仓储实现类
/// </summary>
public class TodoRepository : BaseRepository<Todo,string>, ITodoRepository
{
    public TodoRepository(MarkAgentDbContext context) : base(context)
    {
    }

    public async Task<List<Todo>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Conversation)
            .Include(t => t.UserKey)
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetByUserKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Conversation)
            .Include(t => t.UserKey)
            .Where(t => t.UserKey.Key == userKey)
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ConversationId == conversationId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Todo>> GetByStatusAsync(TodoStatus status, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.Status == status);
        
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }
        
        return await query
            .Include(t => t.Conversation)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetPendingCountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(t => t.UserId == userId && t.Status == TodoStatus.Pending, cancellationToken);
    }

    public async Task<int> GetCompletedCountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(t => t.UserId == userId && t.Status == TodoStatus.Completed, cancellationToken);
    }

    public async Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var todos = await _dbSet.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
        return (
            todos.Count,
            todos.Count(t => t.Status == TodoStatus.Pending),
            todos.Count(t => t.Status == TodoStatus.InProgress),
            todos.Count(t => t.Status == TodoStatus.Completed)
        );
    }

    public async Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatsByUserKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        var todos = await _dbSet
            .Where(t => t.UserKey.Key == userKey)
            .ToListAsync(cancellationToken);
        return (
            todos.Count,
            todos.Count(t => t.Status == TodoStatus.Pending),
            todos.Count(t => t.Status == TodoStatus.InProgress),
            todos.Count(t => t.Status == TodoStatus.Completed)
        );
    }

    public async Task<int> GetCreatedCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate);
        
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetCompletedCountAsync(DateTime startDate, DateTime endDate, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.Status == TodoStatus.Completed && 
                                     t.CompletedAt.HasValue &&
                                     t.CompletedAt >= startDate && 
                                     t.CompletedAt <= endDate);
        
        if (userId.HasValue)
        {
            query = query.Where(t => t.UserId == userId.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// 使用EF Core批量更新API更新Todo状态 - 更高效
    /// </summary>
    public async Task UpdateStatusAsync(string todoId, TodoStatus status, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        if (status == TodoStatus.Completed)
        {
            await ExecuteUpdateAsync(
                t => t.Id == todoId,
                setters => setters
                    .SetProperty(t => t.Status, status)
                    .SetProperty(t => t.UpdatedAt, now)
                    .SetProperty(t => t.CompletedAt, now),
                cancellationToken);
        }
        else
        {
            await ExecuteUpdateAsync(
                t => t.Id == todoId,
                setters => setters
                    .SetProperty(t => t.Status, status)
                    .SetProperty(t => t.UpdatedAt, now),
                cancellationToken);
        }
    }

    /// <summary>
    /// 使用EF Core批量更新API批量更新Todo状态 - 高效的批量操作
    /// </summary>
    public async Task UpdateStatusBatchAsync(List<string> todoIds, TodoStatus status, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        if (status == TodoStatus.Completed)
        {
            await ExecuteUpdateAsync(
                t => todoIds.Contains(t.Id),
                setters => setters
                    .SetProperty(t => t.Status, status)
                    .SetProperty(t => t.UpdatedAt, now)
                    .SetProperty(t => t.CompletedAt, now),
                cancellationToken);
        }
        else
        {
            await ExecuteUpdateAsync(
                t => todoIds.Contains(t.Id),
                setters => setters
                    .SetProperty(t => t.Status, status)
                    .SetProperty(t => t.UpdatedAt, now),
                cancellationToken);
        }
    }

    /// <summary>
    /// 批量完成用户的所有待处理Todo
    /// </summary>
    public async Task<int> CompleteAllPendingByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await ExecuteUpdateAsync(
            t => t.UserId == userId && t.Status == TodoStatus.Pending,
            setters => setters
                .SetProperty(t => t.Status, TodoStatus.Completed)
                .SetProperty(t => t.CompletedAt, now)
                .SetProperty(t => t.UpdatedAt, now),
            cancellationToken);
    }

    /// <summary>
    /// 批量取消用户的所有待处理Todo
    /// </summary>
    public async Task<int> CancelAllPendingByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ExecuteUpdateAsync(
            t => t.UserId == userId && t.Status == TodoStatus.Pending,
            setters => setters
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow),
            cancellationToken);
    }
} 
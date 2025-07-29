using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Repositories;

namespace MarkAgent.Host.Services;

/// <summary>
/// Todo服务实现
/// </summary>
public sealed class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly IUserKeyRepository _userKeyRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserStatisticsRepository _userStatisticsRepository;
    private readonly ILogger<TodoService> _logger;

    public TodoService(
        ITodoRepository todoRepository,
        IUserKeyRepository userKeyRepository,
        IConversationRepository conversationRepository,
        IUserStatisticsRepository userStatisticsRepository,
        ILogger<TodoService> logger)
    {
        _todoRepository = todoRepository;
        _userKeyRepository = userKeyRepository;
        _conversationRepository = conversationRepository;
        _userStatisticsRepository = userStatisticsRepository;
        _logger = logger;
    }

    public async Task<bool> CreateTodoAsync(string userKey, Guid conversationId, List<Todo> todos,
        CancellationToken cancellationToken = default)
    {
        var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
        if (keyInfo == null || keyInfo.Status != KeyStatus.Active)
        {
            _logger.LogWarning("无效的用户密钥: {UserKey}", userKey);
            return false;
        }
        
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation == null || conversation.UserKeyId != keyInfo.Id)
        {
            _logger.LogWarning("对话不存在或无权限: {ConversationId}", conversationId);
            return false;
        }
        
        try
        {
            foreach (var todo in todos)
            {
                todo.UserId = keyInfo.UserId;
                todo.UserKeyId = keyInfo.Id;
                todo.ConversationId = conversationId;
                todo.Status = todo.Status;
                todo.SortOrder = todos.IndexOf(todo);

                await _todoRepository.AddAsync(todo, cancellationToken);
            }

            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 更新用户统计
            await _userStatisticsRepository.IncrementTodosCreatedAsync(
                keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, todos.Count, cancellationToken);
            await _userStatisticsRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("批量创建Todo成功: {UserKey}, {Count}", userKey, todos.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量创建Todo失败: {UserKey}, {Count}", userKey, todos.Count);
            return false;
        }
    }

    public async Task<(bool Success, string Message, Todo? Todo)> CreateTodoAsync(string userKey, Guid conversationId,
        string title, string? description = null, Priority priority = Priority.Medium, DateTime? dueDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 验证用户密钥
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null || keyInfo.Status != KeyStatus.Active)
            {
                return (false, "无效的用户密钥", null);
            }

            // 验证对话是否存在且属于该用户
            var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null || conversation.UserKeyId != keyInfo.Id)
            {
                return (false, "对话不存在或无权限", null);
            }

            var todo = new Todo
            {
                UserId = keyInfo.UserId,
                UserKeyId = keyInfo.Id,
                ConversationId = conversationId,
                Content = description,
                Priority = priority,
                Status = TodoStatus.Pending,
                SortOrder = 0
            };

            await _todoRepository.AddAsync(todo, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 更新用户统计
            await _userStatisticsRepository.IncrementTodosCreatedAsync(
                keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, 1, cancellationToken);
            await _userStatisticsRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Todo创建成功: {UserKey}, {Title}", userKey, title);
            return (true, "Todo创建成功", todo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建Todo失败: {UserKey}, {Title}", userKey, title);
            return (false, "创建失败", null);
        }
    }

    public async Task<List<Todo>> GetTodosByUserKeyAsync(string userKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _todoRepository.GetByUserKeyAsync(userKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取Todo列表失败: {UserKey}", userKey);
            return new List<Todo>();
        }
    }

    public async Task<List<Todo>> GetTodosByConversationAsync(Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _todoRepository.GetByConversationIdAsync(conversationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据对话获取Todo列表失败: {ConversationId}", conversationId);
            return new List<Todo>();
        }
    }

    public async Task<List<Todo>> GetTodosByStatusAsync(string userKey, TodoStatus status,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return new List<Todo>();
            }

            return await _todoRepository.GetByStatusAsync(status, keyInfo.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据状态获取Todo列表失败: {UserKey}, {Status}", userKey, status);
            return new List<Todo>();
        }
    }

    public async Task<(bool Success, string Message)> UpdateTodoStatusAsync(string userKey, string todoId,
        TodoStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥");
            }

            var todo = await _todoRepository.GetByIdAsync(todoId, cancellationToken);
            if (todo == null || todo.UserId != keyInfo.UserId)
            {
                return (false, "Todo不存在或无权限");
            }

            var oldStatus = todo.Status;
            await _todoRepository.UpdateStatusAsync(todoId, status, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 如果状态变为已完成，更新统计
            if (status == TodoStatus.Completed && oldStatus != TodoStatus.Completed)
            {
                await _userStatisticsRepository.IncrementTodosCompletedAsync(
                    keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, 1, cancellationToken);
                await _userStatisticsRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Todo状态更新成功: {UserKey}, {TodoId}, {Status}", userKey, todoId, status);
            return (true, "状态更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新Todo状态失败: {UserKey}, {TodoId}", userKey, todoId);
            return (false, "更新失败");
        }
    }

    public async Task<(bool Success, string Message)> UpdateTodoProgressAsync(string userKey, string todoId, int progress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥");
            }

            var todo = await _todoRepository.GetByIdAsync(todoId, cancellationToken);
            if (todo == null || todo.UserId != keyInfo.UserId)
            {
                return (false, "Todo不存在或无权限");
            }

            var oldStatus = todo.Status;
            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 如果进度达到100%且之前未完成，更新统计
            if (progress >= 100 && oldStatus != TodoStatus.Completed)
            {
                await _userStatisticsRepository.IncrementTodosCompletedAsync(
                    keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, 1, cancellationToken);
                await _userStatisticsRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Todo进度更新成功: {UserKey}, {TodoId}, {Progress}", userKey, todoId, progress);
            return (true, "进度更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新Todo进度失败: {UserKey}, {TodoId}", userKey, todoId);
            return (false, "更新失败");
        }
    }

    public async Task<(bool Success, string Message)> UpdateTodoAsync(string userKey, string todoId, 
        string? content = null, Priority? priority = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥");
            }

            var todo = await _todoRepository.GetByIdAsync(todoId, cancellationToken);
            if (todo == null || todo.UserId != keyInfo.UserId)
            {
                return (false, "Todo不存在或无权限");
            }

            if (content != null)
                todo.Content = content;

            if (priority.HasValue)
                todo.Priority = priority.Value;

            await _todoRepository.UpdateAsync(todo, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Todo更新成功: {UserKey}, {TodoId}", userKey, todoId);
            return (true, "更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新Todo失败: {UserKey}, {TodoId}", userKey, todoId);
            return (false, "更新失败");
        }
    }

    public async Task<(bool Success, string Message)> DeleteTodoAsync(string userKey, string todoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥");
            }

            var todo = await _todoRepository.GetByIdAsync(todoId, cancellationToken);
            if (todo == null || todo.UserId != keyInfo.UserId)
            {
                return (false, "Todo不存在或无权限");
            }

            await _todoRepository.DeleteAsync(todo, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Todo删除成功: {UserKey}, {TodoId}", userKey, todoId);
            return (true, "删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除Todo失败: {UserKey}, {TodoId}", userKey, todoId);
            return (false, "删除失败");
        }
    }

    public async Task<(bool Success, string Message, int AffectedCount)> UpdateTodoStatusBatchAsync(string userKey,
        List<string> todoIds, TodoStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥", 0);
            }

            // 验证所有Todo都属于该用户
            var todos = await _todoRepository.GetListAsync(t => todoIds.Contains(t.Id) && t.UserId == keyInfo.UserId,
                cancellationToken);
            if (todos.Count != todoIds.Count)
            {
                return (false, "部分Todo不存在或无权限", 0);
            }

            // 使用EF Core批量更新API
            await _todoRepository.UpdateStatusBatchAsync(todoIds, status, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 如果状态变为已完成，更新统计
            if (status == TodoStatus.Completed)
            {
                var completedCount = todos.Count(t => t.Status != TodoStatus.Completed);
                if (completedCount > 0)
                {
                    await _userStatisticsRepository.IncrementTodosCompletedAsync(
                        keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, completedCount, cancellationToken);
                    await _userStatisticsRepository.SaveChangesAsync(cancellationToken);
                }
            }

            _logger.LogInformation("Todo批量状态更新成功: {UserKey}, {Count}, {Status}", userKey, todoIds.Count, status);
            return (true, "批量更新成功", todoIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新Todo状态失败: {UserKey}", userKey);
            return (false, "批量更新失败", 0);
        }
    }

    public async Task<(int Total, int Pending, int InProgress, int Completed)> GetTodoStatisticsAsync(
        string userKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _todoRepository.GetTodoStatsByUserKeyAsync(userKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取Todo统计失败: {UserKey}", userKey);
            return (0, 0, 0, 0);
        }
    }

    public async Task<(bool Success, string Message, int CompletedCount)> CompleteAllPendingAsync(string userKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return (false, "无效的用户密钥", 0);
            }

            // 使用TodoRepository中新增的批量完成方法
            var completedCount = await _todoRepository.CompleteAllPendingByUserAsync(keyInfo.UserId, cancellationToken);
            await _todoRepository.SaveChangesAsync(cancellationToken);

            // 更新统计
            if (completedCount > 0)
            {
                await _userStatisticsRepository.IncrementTodosCompletedAsync(
                    keyInfo.UserId, keyInfo.Id, DateTime.UtcNow.Date, completedCount, cancellationToken);
                await _userStatisticsRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("批量完成Todo成功: {UserKey}, {Count}", userKey, completedCount);
            return (true, $"已完成{completedCount}个待处理任务", completedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量完成Todo失败: {UserKey}", userKey);
            return (false, "批量完成失败", 0);
        }
    }

    public async Task<Todo?> GetTodoByIdAsync(string userKey, string todoId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var keyInfo = await _userKeyRepository.GetByKeyAsync(userKey, cancellationToken);
            if (keyInfo == null)
            {
                return null;
            }

            var todo = await _todoRepository.GetByIdAsync(todoId, cancellationToken);
            if (todo == null || todo.UserId != keyInfo.UserId)
            {
                return null;
            }

            return todo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取Todo详情失败: {UserKey}, {TodoId}", userKey, todoId);
            return null;
        }
    }
}
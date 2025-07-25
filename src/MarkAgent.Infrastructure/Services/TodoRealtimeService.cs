using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;
using MarkAgent.Application.DTOs.Todo;
using MarkAgent.Application.Services;
using Microsoft.Extensions.Logging;

namespace MarkAgent.Infrastructure.Services;

public class TodoRealtimeService : ITodoRealtimeService
{
    private readonly ILogger<TodoRealtimeService> _logger;
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<string>> _userStreams = new();

    public TodoRealtimeService(ILogger<TodoRealtimeService> logger)
    {
        _logger = logger;
    }

    public async Task NotifyTodoCreatedAsync(Guid userId, TodoItemDto todo)
    {
        var notification = JsonSerializer.Serialize(new
        {
            type = "todo_created",
            data = todo,
            timestamp = DateTime.UtcNow
        });

        await EnqueueMessage(userId, notification);
    }

    public async Task NotifyTodoUpdatedAsync(Guid userId, TodoItemDto todo)
    {
        var notification = JsonSerializer.Serialize(new
        {
            type = "todo_updated",
            data = todo,
            timestamp = DateTime.UtcNow
        });

        await EnqueueMessage(userId, notification);
    }

    public async Task NotifyTodoDeletedAsync(Guid userId, Guid todoId)
    {
        var notification = JsonSerializer.Serialize(new
        {
            type = "todo_deleted",
            data = new { id = todoId },
            timestamp = DateTime.UtcNow
        });

        await EnqueueMessage(userId, notification);
    }

    public async Task NotifyTodoStatusChangedAsync(Guid userId, Guid todoId, string newStatus)
    {
        var notification = JsonSerializer.Serialize(new
        {
            type = "todo_status_changed",
            data = new { id = todoId, status = newStatus },
            timestamp = DateTime.UtcNow
        });

        await EnqueueMessage(userId, notification);
    }

    public async Task<IAsyncEnumerable<string>> GetTodoUpdatesStreamAsync(Guid userId, CancellationToken cancellationToken)
    {
        return GetUpdatesStream(userId, cancellationToken);
    }

    private async IAsyncEnumerable<string> GetUpdatesStream(Guid userId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var userQueue = _userStreams.GetOrAdd(userId, _ => new ConcurrentQueue<string>());

        // Send initial connection message
        yield return JsonSerializer.Serialize(new
        {
            type = "connected",
            timestamp = DateTime.UtcNow,
            message = "Connected to todo updates stream"
        });

        while (!cancellationToken.IsCancellationRequested)
        {
            if (userQueue.TryDequeue(out var message))
            {
                yield return message;
            }
            else
            {
                // Send heartbeat every 30 seconds
                yield return JsonSerializer.Serialize(new
                {
                    type = "heartbeat",
                    timestamp = DateTime.UtcNow
                });

                await Task.Delay(30000, cancellationToken);
            }
        }

        // Cleanup when client disconnects
        _userStreams.TryRemove(userId, out _);
    }

    private Task EnqueueMessage(Guid userId, string message)
    {
        var userQueue = _userStreams.GetOrAdd(userId, _ => new ConcurrentQueue<string>());
        userQueue.Enqueue(message);

        // Prevent queue from growing too large
        while (userQueue.Count > 100)
        {
            userQueue.TryDequeue(out _);
        }

        _logger.LogDebug("Enqueued message for user {UserId}: {Message}", userId, message);
        return Task.CompletedTask;
    }
}
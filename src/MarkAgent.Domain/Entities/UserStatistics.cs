using MarkAgent.Domain.Common;

namespace MarkAgent.Domain.Entities;

public class UserStatistics : Entity
{
    public Guid UserId { get; private set; }
    public int TotalTodoCreated { get; private set; }
    public int TotalTodoCompleted { get; private set; }
    public int TotalConversationSessions { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    private UserStatistics() { } // For EF Core

    public UserStatistics(Guid userId)
    {
        UserId = userId;
        TotalTodoCreated = 0;
        TotalTodoCompleted = 0;
        TotalConversationSessions = 0;
        LastActivityAt = DateTime.UtcNow;
    }

    public void IncrementTodoCreated()
    {
        TotalTodoCreated++;
        LastActivityAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void IncrementTodoCompleted()
    {
        TotalTodoCompleted++;
        LastActivityAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void IncrementConversationSessions()
    {
        TotalConversationSessions++;
        LastActivityAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Reset()
    {
        TotalTodoCreated = 0;
        TotalTodoCompleted = 0;
        TotalConversationSessions = 0;
        LastActivityAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}
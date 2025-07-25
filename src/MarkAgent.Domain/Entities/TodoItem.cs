using MarkAgent.Domain.Common;
using MarkAgent.Domain.Enums;

namespace MarkAgent.Domain.Entities;

public class TodoItem : Entity
{
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TodoStatus Status { get; private set; }
    public int Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Foreign keys
    public Guid UserId { get; private set; }
    public Guid ConversationSessionId { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public ConversationSession ConversationSession { get; private set; } = null!;

    private TodoItem() { } // For EF Core

    public TodoItem(string title, string? description, Guid userId, Guid conversationSessionId, int priority = 0, DateTime? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        Title = title;
        Description = description;
        Status = TodoStatus.Pending;
        Priority = priority;
        DueDate = dueDate;
        UserId = userId;
        ConversationSessionId = conversationSessionId;
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        Title = title;
        UpdateTimestamp();
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdateTimestamp();
    }

    public void UpdatePriority(int priority)
    {
        Priority = priority;
        UpdateTimestamp();
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
        UpdateTimestamp();
    }

    public void MarkAsInProgress()
    {
        Status = TodoStatus.InProgress;
        UpdateTimestamp();
    }

    public void MarkAsCompleted()
    {
        Status = TodoStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void MarkAsCancelled()
    {
        Status = TodoStatus.Cancelled;
        UpdateTimestamp();
    }

    public void ResetStatus()
    {
        Status = TodoStatus.Pending;
        CompletedAt = null;
        UpdateTimestamp();
    }
}
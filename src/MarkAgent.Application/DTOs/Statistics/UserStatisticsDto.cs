namespace MarkAgent.Application.DTOs.Statistics;

public class UserStatisticsDto
{
    public Guid UserId { get; set; }
    public int TotalTodoCreated { get; set; }
    public int TotalTodoCompleted { get; set; }
    public int TotalConversationSessions { get; set; }
    public DateTime LastActivityAt { get; set; }
    public double CompletionRate => TotalTodoCreated > 0 ? (double)TotalTodoCompleted / TotalTodoCreated * 100 : 0;
}
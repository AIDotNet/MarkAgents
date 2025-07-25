namespace MarkAgent.Application.DTOs.Statistics;

public class SystemStatisticsDto
{
    public int TotalUsers { get; set; }
    public int TotalTodos { get; set; }
    public int TotalCompletedTodos { get; set; }
    public int TotalActiveSessions { get; set; }
    public double OverallCompletionRate => TotalTodos > 0 ? (double)TotalCompletedTodos / TotalTodos * 100 : 0;
}
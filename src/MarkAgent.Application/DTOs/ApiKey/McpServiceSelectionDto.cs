namespace MarkAgent.Application.DTOs.ApiKey;

public class McpServiceSelectionDto
{
    public Guid ServiceId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Configuration { get; set; }
}

public class McpServiceAssignmentDto : McpServiceSelectionDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsSystemService { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
}
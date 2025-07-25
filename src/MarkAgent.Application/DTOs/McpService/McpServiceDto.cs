namespace MarkAgent.Application.DTOs.McpService;

public class McpServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ServiceClass { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsSystemService { get; set; }
    public int SortOrder { get; set; }
    public string? ConfigurationSchema { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
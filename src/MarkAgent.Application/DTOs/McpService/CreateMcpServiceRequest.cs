using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.McpService;

public class CreateMcpServiceRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Version { get; set; } = string.Empty;

    [Required]
    public string ServiceClass { get; set; } = string.Empty;

    public bool IsSystemService { get; set; } = false;

    public int SortOrder { get; set; } = 0;

    public string? ConfigurationSchema { get; set; }
}
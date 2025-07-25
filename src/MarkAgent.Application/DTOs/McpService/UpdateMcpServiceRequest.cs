using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.McpService;

public class UpdateMcpServiceRequest
{
    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? Version { get; set; }

    public bool? IsActive { get; set; }

    public int? SortOrder { get; set; }

    public string? ConfigurationSchema { get; set; }
}
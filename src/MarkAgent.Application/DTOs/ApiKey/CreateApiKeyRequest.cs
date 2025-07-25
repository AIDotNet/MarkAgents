using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.ApiKey;

public class CreateApiKeyRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? CustomKey { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int? RateLimit { get; set; }

    public List<string>? AllowedIpAddresses { get; set; }

    public List<McpServiceSelectionDto> McpServices { get; set; } = new();
}
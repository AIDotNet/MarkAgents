using System.ComponentModel.DataAnnotations;

namespace MarkAgent.Application.DTOs.ApiKey;

public class UpdateApiKeyRequest
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int? RateLimit { get; set; }

    public List<string>? AllowedIpAddresses { get; set; }

    public bool? IsActive { get; set; }
}
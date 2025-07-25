using MarkAgent.Domain.Common;

namespace MarkAgent.Domain.Entities;

public class UserKeyMcpService : Entity
{
    public Guid UserId { get; private set; }
    public string UserKey { get; private set; }
    public Guid McpServiceId { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? Configuration { get; private set; } // JSON configuration for this service
    public DateTime? LastUsedAt { get; private set; }
    public int UsageCount { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public McpService McpService { get; private set; } = null!;

    private UserKeyMcpService() { } // For EF Core

    public UserKeyMcpService(Guid userId, string userKey, Guid mcpServiceId, bool isEnabled = true, string? configuration = null)
    {
        if (string.IsNullOrWhiteSpace(userKey))
            throw new ArgumentException("User key cannot be null or empty.", nameof(userKey));

        UserId = userId;
        UserKey = userKey;
        McpServiceId = mcpServiceId;
        IsEnabled = isEnabled;
        Configuration = configuration;
        UsageCount = 0;
    }

    public void Enable()
    {
        IsEnabled = true;
        UpdateTimestamp();
    }

    public void Disable()
    {
        IsEnabled = false;
        UpdateTimestamp();
    }

    public void UpdateConfiguration(string? configuration)
    {
        Configuration = configuration;
        UpdateTimestamp();
    }

    public void RecordUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void ResetUsageCount()
    {
        UsageCount = 0;
        UpdateTimestamp();
    }
}
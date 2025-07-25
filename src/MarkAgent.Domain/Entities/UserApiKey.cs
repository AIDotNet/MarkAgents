using MarkAgent.Domain.Common;
using MarkAgent.Domain.ValueObjects;

namespace MarkAgent.Domain.Entities;

public class UserApiKey : Entity
{
    public Guid UserId { get; private set; }
    public UserKey ApiKey { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public int UsageCount { get; private set; }
    public string? AllowedIpAddresses { get; private set; } // JSON array of IP addresses
    public int? RateLimit { get; private set; } // Requests per minute

    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<UserKeyMcpService> McpServices { get; private set; } = new List<UserKeyMcpService>();

    private UserApiKey() { } // For EF Core

    public UserApiKey(
        Guid userId, 
        string name, 
        string? description = null, 
        DateTime? expiresAt = null,
        string? allowedIpAddresses = null,
        int? rateLimit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        UserId = userId;
        ApiKey = ValueObjects.UserKey.GenerateNew();
        Name = name;
        Description = description;
        IsActive = true;
        ExpiresAt = expiresAt;
        UsageCount = 0;
        AllowedIpAddresses = allowedIpAddresses;
        RateLimit = rateLimit;
    }

    public UserApiKey(
        Guid userId, 
        UserKey customKey,
        string name, 
        string? description = null, 
        DateTime? expiresAt = null,
        string? allowedIpAddresses = null,
        int? rateLimit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        UserId = userId;
        ApiKey = customKey;
        Name = name;
        Description = description;
        IsActive = true;
        ExpiresAt = expiresAt;
        UsageCount = 0;
        AllowedIpAddresses = allowedIpAddresses;
        RateLimit = rateLimit;
    }

    public void UpdateInfo(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Name = name;
        Description = description;
        UpdateTimestamp();
    }

    public void SetExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
        UpdateTimestamp();
    }

    public void UpdateSecuritySettings(string? allowedIpAddresses, int? rateLimit)
    {
        AllowedIpAddresses = allowedIpAddresses;
        RateLimit = rateLimit;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void RecordUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;

    public bool IsValid => IsActive && !IsExpired;

    public void AddMcpService(Guid mcpServiceId, bool isEnabled = true, string? configuration = null)
    {
        var existingService = McpServices.FirstOrDefault(s => s.McpServiceId == mcpServiceId);
        if (existingService != null)
        {
            if (isEnabled)
                existingService.Enable();
            else
                existingService.Disable();
            
            existingService.UpdateConfiguration(configuration);
        }
        else
        {
            var userKeyMcpService = new UserKeyMcpService(UserId, ApiKey.Value, mcpServiceId, isEnabled, configuration);
            McpServices.Add(userKeyMcpService);
        }
        UpdateTimestamp();
    }

    public void RemoveMcpService(Guid mcpServiceId)
    {
        var service = McpServices.FirstOrDefault(s => s.McpServiceId == mcpServiceId);
        if (service != null)
        {
            McpServices.Remove(service);
            UpdateTimestamp();
        }
    }

    public bool HasAccessToMcpService(Guid mcpServiceId)
    {
        return McpServices.Any(s => s.McpServiceId == mcpServiceId && s.IsEnabled);
    }
}
using MarkAgent.Domain.Common;

namespace MarkAgent.Domain.Entities;

public class McpService : Entity
{
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string Description { get; private set; }
    public string Version { get; private set; }
    public string ServiceClass { get; private set; } // 服务实现类的完整名称
    public bool IsActive { get; private set; }
    public bool IsSystemService { get; private set; } // 系统服务不能被禁用
    public int SortOrder { get; private set; }
    public string? ConfigurationSchema { get; private set; } // JSON Schema for service configuration

    // Navigation properties
    public ICollection<UserKeyMcpService> UserKeyMcpServices { get; private set; } = new List<UserKeyMcpService>();

    private McpService() { } // For EF Core

    public McpService(
        string name, 
        string displayName, 
        string description, 
        string version,
        string serviceClass,
        bool isSystemService = false,
        int sortOrder = 0,
        string? configurationSchema = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(serviceClass))
            throw new ArgumentException("Service class cannot be null or empty.", nameof(serviceClass));

        Name = name;
        DisplayName = displayName;
        Description = description;
        Version = version;
        ServiceClass = serviceClass;
        IsActive = true;
        IsSystemService = isSystemService;
        SortOrder = sortOrder;
        ConfigurationSchema = configurationSchema;
    }

    public void UpdateInfo(string displayName, string description, string? configurationSchema = null)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be null or empty.", nameof(displayName));

        DisplayName = displayName;
        Description = description;
        ConfigurationSchema = configurationSchema;
        UpdateTimestamp();
    }

    public void UpdateVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be null or empty.", nameof(version));

        Version = version;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (IsSystemService)
            throw new InvalidOperationException("System services cannot be deactivated.");

        IsActive = false;
        UpdateTimestamp();
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateTimestamp();
    }
}
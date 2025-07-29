using MarkAgent.Host.Domain.Entities;

namespace MarkAgent.Host.Domain.Events;

/// <summary>
/// 统计事件基类
/// </summary>
public abstract class StatisticsEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string EventId { get; init; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 工具使用事件
/// </summary>
public class ToolUsageEvent : StatisticsEvent
{
    public required string ToolName { get; init; }
    public required string SessionId { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public int InputSize { get; init; }
    public int OutputSize { get; init; }
    public string? ParametersJson { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

/// <summary>
/// 客户端连接事件
/// </summary>
public class ClientConnectionEvent : StatisticsEvent
{
    public required string ClientName { get; init; }
    public string? ClientVersion { get; init; }
    public string? ClientTitle { get; init; }
    public required string SessionId { get; init; }
    public required DateTime ConnectionTime { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? ProtocolVersion { get; init; }
    public string? ClientCapabilities { get; init; }
}

/// <summary>
/// 客户端状态更新事件
/// </summary>
public class ClientStatusUpdateEvent : StatisticsEvent
{
    public required string SessionId { get; init; }
    public required ClientConnectionStatus Status { get; init; }
    public DateTime? DisconnectionTime { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 客户端工具使用增量事件
/// </summary>
public class ClientToolUsageIncrementEvent : StatisticsEvent
{
    public required string SessionId { get; init; }
}


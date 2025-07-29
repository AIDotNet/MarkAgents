namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 客户端连接记录
/// </summary>
public class ClientConnectionRecord
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 客户端名称
    /// </summary>
    public required string ClientName { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// 客户端标题/显示名称
    /// </summary>
    public string? ClientTitle { get; set; }

    /// <summary>
    /// 会话ID（用于关联工具使用记录）
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    public DateTime ConnectionTime { get; set; }

    /// <summary>
    /// 断开连接时间（可选）
    /// </summary>
    public DateTime? DisconnectionTime { get; set; }

    /// <summary>
    /// 连接持续时间（秒）
    /// </summary>
    public long? ConnectionDurationSeconds { get; set; }

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent信息
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 协议版本
    /// </summary>
    public string? ProtocolVersion { get; set; }

    /// <summary>
    /// 客户端能力信息（JSON格式）
    /// </summary>
    public string? ClientCapabilities { get; set; }

    /// <summary>
    /// 连接状态
    /// </summary>
    public ClientConnectionStatus Status { get; set; } = ClientConnectionStatus.Connected;

    /// <summary>
    /// 在此连接中使用的工具次数
    /// </summary>
    public int ToolUsageCount { get; set; } = 0;

    /// <summary>
    /// 错误信息（如果连接失败）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 客户端连接状态
/// </summary>
public enum ClientConnectionStatus
{
    /// <summary>
    /// 已连接
    /// </summary>
    Connected = 0,

    /// <summary>
    /// 已断开
    /// </summary>
    Disconnected = 1,

    /// <summary>
    /// 连接失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 3
}
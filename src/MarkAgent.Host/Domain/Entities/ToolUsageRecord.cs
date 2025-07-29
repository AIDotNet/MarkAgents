namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 工具使用记录
/// </summary>
public class ToolUsageRecord
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 工具名称
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// 会话ID（可用于追踪同一用户会话）
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 开始执行时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束执行时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 执行持续时间（毫秒）
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// 是否执行成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误信息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 输入数据大小（字节）
    /// </summary>
    public int InputSize { get; set; }

    /// <summary>
    /// 输出数据大小（字节）
    /// </summary>
    public int OutputSize { get; set; }

    /// <summary>
    /// 工具参数JSON（用于分析使用模式）
    /// </summary>
    public string? ParametersJson { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP地址（可选）
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理信息（可选）
    /// </summary>
    public string? UserAgent { get; set; }
}
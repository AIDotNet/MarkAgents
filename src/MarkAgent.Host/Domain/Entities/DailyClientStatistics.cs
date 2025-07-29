namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 每日客户端统计汇总
/// </summary>
public class DailyClientStatistics
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 统计日期
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// 客户端名称
    /// </summary>
    public required string ClientName { get; set; }

    /// <summary>
    /// 客户端版本（主要版本）
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// 总连接次数
    /// </summary>
    public int TotalConnections { get; set; }

    /// <summary>
    /// 成功连接次数
    /// </summary>
    public int SuccessfulConnections { get; set; }

    /// <summary>
    /// 失败连接次数
    /// </summary>
    public int FailedConnections { get; set; }

    /// <summary>
    /// 总连接时长（秒）
    /// </summary>
    public long TotalConnectionDurationSeconds { get; set; }

    /// <summary>
    /// 平均连接时长（秒）
    /// </summary>
    public double AverageConnectionDurationSeconds { get; set; }

    /// <summary>
    /// 最长连接时长（秒）
    /// </summary>
    public long MaxConnectionDurationSeconds { get; set; }

    /// <summary>
    /// 最短连接时长（秒）
    /// </summary>
    public long MinConnectionDurationSeconds { get; set; }

    /// <summary>
    /// 在此客户端上的工具使用总次数
    /// </summary>
    public int TotalToolUsages { get; set; }

    /// <summary>
    /// 平均每连接的工具使用次数
    /// </summary>
    public double AverageToolUsagesPerConnection { get; set; }

    /// <summary>
    /// 独立会话数量
    /// </summary>
    public int UniqueSessionCount { get; set; }

    /// <summary>
    /// 首次连接时间
    /// </summary>
    public DateTime FirstConnectionTime { get; set; }

    /// <summary>
    /// 最后连接时间
    /// </summary>
    public DateTime LastConnectionTime { get; set; }

    /// <summary>
    /// 连接成功率（百分比）
    /// </summary>
    public double ConnectionSuccessRate => TotalConnections > 0 
        ? (double)SuccessfulConnections / TotalConnections * 100 
        : 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
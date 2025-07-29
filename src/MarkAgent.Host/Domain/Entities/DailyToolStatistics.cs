namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// 每日工具使用统计汇总
/// </summary>
public class DailyToolStatistics
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
    /// 工具名称
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// 总使用次数
    /// </summary>
    public int TotalUsageCount { get; set; }

    /// <summary>
    /// 成功执行次数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败执行次数
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 平均执行时间（毫秒）
    /// </summary>
    public double AverageExecutionTimeMs { get; set; }

    /// <summary>
    /// 最短执行时间（毫秒）
    /// </summary>
    public long MinExecutionTimeMs { get; set; }

    /// <summary>
    /// 最长执行时间（毫秒）
    /// </summary>
    public long MaxExecutionTimeMs { get; set; }

    /// <summary>
    /// 总输入数据大小（字节）
    /// </summary>
    public long TotalInputSize { get; set; }

    /// <summary>
    /// 总输出数据大小（字节）
    /// </summary>
    public long TotalOutputSize { get; set; }

    /// <summary>
    /// 独立会话数量
    /// </summary>
    public int UniqueSessionCount { get; set; }

    /// <summary>
    /// 成功率（百分比）
    /// </summary>
    public double SuccessRate => TotalUsageCount > 0 ? (double)SuccessCount / TotalUsageCount * 100 : 0;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
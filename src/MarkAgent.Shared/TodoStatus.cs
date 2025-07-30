namespace MarkAgent.Host.Domain.Entities;

/// <summary>
/// Todo状态枚举
/// </summary>
public enum TodoStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 进行中
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2
}

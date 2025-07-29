using MarkAgent.Host.Domain.Events;

namespace MarkAgent.Host.Domain.Services;

/// <summary>
/// 统计数据Channel服务接口
/// </summary>
public interface IStatisticsChannelService
{
    /// <summary>
    /// 写入工具使用事件
    /// </summary>
    /// <param name="toolUsageEvent">工具使用事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task WriteToolUsageEventAsync(ToolUsageEvent toolUsageEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入客户端连接事件
    /// </summary>
    /// <param name="connectionEvent">客户端连接事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task WriteClientConnectionEventAsync(ClientConnectionEvent connectionEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入客户端状态更新事件
    /// </summary>
    /// <param name="statusUpdateEvent">客户端状态更新事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task WriteClientStatusUpdateEventAsync(ClientStatusUpdateEvent statusUpdateEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 写入客户端工具使用增量事件
    /// </summary>
    /// <param name="incrementEvent">客户端工具使用增量事件</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task WriteClientToolUsageIncrementEventAsync(ClientToolUsageIncrementEvent incrementEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取Channel统计信息
    /// </summary>
    /// <returns>Channel统计信息</returns>
    ChannelStatistics GetChannelStatistics();
}

/// <summary>
/// Channel统计信息
/// </summary>
public class ChannelStatistics
{
    public int PendingEventCount { get; set; }
    public long TotalProcessedEvents { get; set; }
    public long TotalFailedEvents { get; set; }
    public DateTime LastProcessedTime { get; set; }
    public bool IsHealthy { get; set; }
}
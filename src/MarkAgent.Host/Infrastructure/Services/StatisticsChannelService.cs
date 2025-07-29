using System.Threading.Channels;
using MarkAgent.Host.Domain.Events;
using MarkAgent.Host.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MarkAgent.Host.Infrastructure.Services;

/// <summary>
/// 统计数据Channel服务实现
/// </summary>
public class StatisticsChannelService : IStatisticsChannelService, IDisposable
{
    private readonly Channel<StatisticsEvent> _channel;
    private readonly ChannelWriter<StatisticsEvent> _writer;
    private readonly ILogger<StatisticsChannelService> _logger;
    
    // 统计信息
    private long _totalProcessedEvents = 0;
    private long _totalFailedEvents = 0;
    private DateTime _lastProcessedTime = DateTime.UtcNow;
    private volatile bool _isHealthy = true;

    public StatisticsChannelService(ILogger<StatisticsChannelService> logger)
    {
        _logger = logger;
        
        // 创建有界Channel，防止内存泄漏
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        };

        _channel = Channel.CreateBounded<StatisticsEvent>(options);
        _writer = _channel.Writer;
    }

    /// <summary>
    /// 获取Channel读取器，用于后台服务读取事件
    /// </summary>
    public ChannelReader<StatisticsEvent> Reader => _channel.Reader;

    public async Task WriteToolUsageEventAsync(ToolUsageEvent toolUsageEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _writer.WriteAsync(toolUsageEvent, cancellationToken);
            _logger.LogDebug("Tool usage event written to channel: {ToolName}", toolUsageEvent.ToolName);
        }
        catch (Exception ex)
        {
            _isHealthy = false;
            _logger.LogError(ex, "Failed to write tool usage event to channel");
            throw;
        }
    }

    public async Task WriteClientConnectionEventAsync(ClientConnectionEvent connectionEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _writer.WriteAsync(connectionEvent, cancellationToken);
            _logger.LogDebug("Client connection event written to channel: {ClientName}", connectionEvent.ClientName);
        }
        catch (Exception ex)
        {
            _isHealthy = false;
            _logger.LogError(ex, "Failed to write client connection event to channel");
            throw;
        }
    }

    public async Task WriteClientStatusUpdateEventAsync(ClientStatusUpdateEvent statusUpdateEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _writer.WriteAsync(statusUpdateEvent, cancellationToken);
            _logger.LogDebug("Client status update event written to channel: {SessionId}", statusUpdateEvent.SessionId);
        }
        catch (Exception ex)
        {
            _isHealthy = false;
            _logger.LogError(ex, "Failed to write client status update event to channel");
            throw;
        }
    }

    public async Task WriteClientToolUsageIncrementEventAsync(ClientToolUsageIncrementEvent incrementEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await _writer.WriteAsync(incrementEvent, cancellationToken);
            _logger.LogDebug("Client tool usage increment event written to channel: {SessionId}", incrementEvent.SessionId);
        }
        catch (Exception ex)
        {
            _isHealthy = false;
            _logger.LogError(ex, "Failed to write client tool usage increment event to channel");
            throw;
        }
    }

    public ChannelStatistics GetChannelStatistics()
    {
        return new ChannelStatistics
        {
            PendingEventCount = _channel.Reader.CanCount ? _channel.Reader.Count : -1,
            TotalProcessedEvents = _totalProcessedEvents,
            TotalFailedEvents = _totalFailedEvents,
            LastProcessedTime = _lastProcessedTime,
            IsHealthy = _isHealthy
        };
    }

    /// <summary>
    /// 内部方法，用于更新统计信息
    /// </summary>
    internal void UpdateStatistics(bool isSuccess)
    {
        if (isSuccess)
        {
            Interlocked.Increment(ref _totalProcessedEvents);
            _isHealthy = true;
        }
        else
        {
            Interlocked.Increment(ref _totalFailedEvents);
        }
        
        _lastProcessedTime = DateTime.UtcNow;
    }

    public void Dispose()
    {
        _writer.Complete();
        _logger.LogInformation("Statistics channel service disposed");
    }
}
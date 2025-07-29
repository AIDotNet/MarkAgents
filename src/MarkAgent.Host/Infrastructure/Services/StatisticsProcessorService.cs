using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Domain.Events;
using MarkAgent.Host.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarkAgent.Host.Infrastructure.Services;

/// <summary>
/// 统计数据处理后台服务
/// </summary>
public class StatisticsProcessorService : BackgroundService
{
    private readonly StatisticsChannelService _channelService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StatisticsProcessorService> _logger;

    public StatisticsProcessorService(
        StatisticsChannelService channelService,
        IServiceScopeFactory scopeFactory,
        ILogger<StatisticsProcessorService> logger)
    {
        _channelService = channelService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Statistics processor service started");

        await foreach (var statisticsEvent in _channelService.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessEventAsync(statisticsEvent, stoppingToken);
                _channelService.UpdateStatistics(isSuccess: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process statistics event: {EventType}", statisticsEvent.GetType().Name);
                _channelService.UpdateStatistics(isSuccess: false);
            }
        }

        _logger.LogInformation("Statistics processor service stopped");
    }

    private async Task ProcessEventAsync(StatisticsEvent statisticsEvent, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        switch (statisticsEvent)
        {
            case ToolUsageEvent toolUsageEvent:
                await ProcessToolUsageEventAsync(scope, toolUsageEvent, cancellationToken);
                break;
                
            case ClientConnectionEvent connectionEvent:
                await ProcessClientConnectionEventAsync(scope, connectionEvent, cancellationToken);
                break;
                
            case ClientStatusUpdateEvent statusUpdateEvent:
                await ProcessClientStatusUpdateEventAsync(scope, statusUpdateEvent, cancellationToken);
                break;
                
            case ClientToolUsageIncrementEvent incrementEvent:
                await ProcessClientToolUsageIncrementEventAsync(scope, incrementEvent, cancellationToken);
                break;
                
            default:
                _logger.LogWarning("Unknown statistics event type: {EventType}", statisticsEvent.GetType().Name);
                break;
        }
    }

    private async Task ProcessToolUsageEventAsync(IServiceScope scope, ToolUsageEvent toolUsageEvent, CancellationToken cancellationToken)
    {
        var statisticsService = scope.ServiceProvider.GetRequiredService<IToolStatisticsService>();
        
        await statisticsService.RecordToolUsageAsync(
            toolName: toolUsageEvent.ToolName,
            startTime: toolUsageEvent.StartTime,
            endTime: toolUsageEvent.EndTime,
            isSuccess: toolUsageEvent.IsSuccess,
            errorMessage: toolUsageEvent.ErrorMessage,
            inputSize: toolUsageEvent.InputSize,
            outputSize: toolUsageEvent.OutputSize,
            parametersJson: toolUsageEvent.ParametersJson,
            sessionId: toolUsageEvent.SessionId,
            ipAddress: toolUsageEvent.IpAddress,
            userAgent: toolUsageEvent.UserAgent
        );

        _logger.LogDebug("Processed tool usage event: {ToolName} for session {SessionId}", 
            toolUsageEvent.ToolName, toolUsageEvent.SessionId);
    }

    private async Task ProcessClientConnectionEventAsync(IServiceScope scope, ClientConnectionEvent connectionEvent, CancellationToken cancellationToken)
    {
        var statisticsService = scope.ServiceProvider.GetRequiredService<IToolStatisticsService>();
        
        await statisticsService.RecordClientConnectionAsync(
            clientName: connectionEvent.ClientName,
            clientVersion: connectionEvent.ClientVersion,
            clientTitle: connectionEvent.ClientTitle,
            ipAddress: connectionEvent.IpAddress,
            userAgent: connectionEvent.UserAgent,
            protocolVersion: connectionEvent.ProtocolVersion,
            clientCapabilities: connectionEvent.ClientCapabilities
        );

        _logger.LogDebug("Processed client connection event: {ClientName} v{ClientVersion}", 
            connectionEvent.ClientName, connectionEvent.ClientVersion);
    }

    private async Task ProcessClientStatusUpdateEventAsync(IServiceScope scope, ClientStatusUpdateEvent statusUpdateEvent, CancellationToken cancellationToken)
    {
        var statisticsService = scope.ServiceProvider.GetRequiredService<IToolStatisticsService>();
        
        await statisticsService.UpdateClientConnectionStatusAsync(
            sessionId: statusUpdateEvent.SessionId,
            status: statusUpdateEvent.Status,
            disconnectionTime: statusUpdateEvent.DisconnectionTime,
            errorMessage: statusUpdateEvent.ErrorMessage
        );

        _logger.LogDebug("Processed client status update event: {SessionId} -> {Status}", 
            statusUpdateEvent.SessionId, statusUpdateEvent.Status);
    }

    private async Task ProcessClientToolUsageIncrementEventAsync(IServiceScope scope, ClientToolUsageIncrementEvent incrementEvent, CancellationToken cancellationToken)
    {
        var statisticsService = scope.ServiceProvider.GetRequiredService<IToolStatisticsService>();
        
        await statisticsService.IncrementClientToolUsageAsync(incrementEvent.SessionId);

        _logger.LogDebug("Processed client tool usage increment event: {SessionId}", incrementEvent.SessionId);
    }
}
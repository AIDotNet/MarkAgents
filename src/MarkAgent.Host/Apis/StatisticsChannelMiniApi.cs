using MarkAgent.Host.Domain.Services;

namespace MarkAgent.Host.Apis;

/// <summary>
/// 统计Channel监控MiniAPI
/// </summary>
public static class StatisticsChannelMiniApi
{
    /// <summary>
    /// 配置统计Channel相关的API端点
    /// </summary>
    public static void MapStatisticsChannelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/statistics/channel")
            .WithTags("Statistics Channel")
            .WithOpenApi();

        // 获取Channel统计信息
        group.MapGet("/status", (IStatisticsChannelService channelService) =>
        {
            var statistics = channelService.GetChannelStatistics();
            return Results.Ok(statistics);
        })
        .WithName("GetChannelStatus")
        .WithSummary("获取Channel统计信息")
        .WithDescription("获取统计数据Channel的当前状态和统计信息");

        // 健康检查端点
        group.MapGet("/health", (IStatisticsChannelService channelService) =>
        {
            var statistics = channelService.GetChannelStatistics();
            
            var result = new
            {
                status = statistics.IsHealthy ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow,
                details = new
                {
                    pendingEvents = statistics.PendingEventCount,
                    totalProcessed = statistics.TotalProcessedEvents,
                    totalFailed = statistics.TotalFailedEvents,
                    lastProcessed = statistics.LastProcessedTime,
                    successRate = statistics.TotalProcessedEvents + statistics.TotalFailedEvents > 0 
                        ? (double)statistics.TotalProcessedEvents / (statistics.TotalProcessedEvents + statistics.TotalFailedEvents) * 100 
                        : 0
                }
            };

            return Results.Ok(result);
        })
        .WithName("ChannelHealthCheck")
        .WithSummary("Channel健康检查")
        .WithDescription("检查统计数据Channel的健康状态和处理性能");
    }
}
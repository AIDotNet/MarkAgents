using MarkAgent.Host.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarkAgent.Host.Apis;

/// <summary>
/// 统计数据API
/// </summary>
[ApiController]
[Route("api/statistics")]
public class StatisticsApi : ControllerBase
{
    private readonly IToolStatisticsService _statisticsService;

    public StatisticsApi(IToolStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// 获取概览统计数据（用于仪表板卡片）
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewStatistics>> GetOverviewStatistics()
    {
        try
        {
            var overview = await _statisticsService.GetOverviewStatisticsAsync();
            return Ok(overview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取工具使用趋势数据（用于趋势图表）
    /// </summary>
    [HttpGet("trend")]
    public async Task<ActionResult<List<TrendDataPoint>>> GetUsageTrend([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var trendData = await _statisticsService.GetUsageTrendDataAsync(days);
            return Ok(trendData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取工具使用分布数据（用于饼图/环形图）
    /// </summary>
    [HttpGet("distribution")]
    public async Task<ActionResult<List<ToolUsageDistribution>>> GetUsageDistribution([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var distribution = await _statisticsService.GetToolUsageDistributionAsync(days);
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取工具成功率统计数据（用于条形图）
    /// </summary>
    [HttpGet("success-rate")]
    public async Task<ActionResult<List<ToolSuccessRateStats>>> GetSuccessRateStats([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var successRateStats = await _statisticsService.GetToolSuccessRateStatsAsync(days);
            return Ok(successRateStats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取工具性能统计数据（用于性能图表）
    /// </summary>
    [HttpGet("performance")]
    public async Task<ActionResult<List<ToolPerformanceStats>>> GetPerformanceStats([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var performanceStats = await _statisticsService.GetToolPerformanceStatsAsync(days);
            return Ok(performanceStats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取最近活动数据（用于活动时间线）
    /// </summary>
    [HttpGet("recent-activities")]
    public async Task<ActionResult<List<RecentActivityItem>>> GetRecentActivities([FromQuery] int count = 20)
    {
        try
        {
            if (count < 1 || count > 100)
                return BadRequest(new { error = "Count must be between 1 and 100" });

            var activities = await _statisticsService.GetRecentActivitiesAsync(count);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取使用热力图数据（按小时统计使用情况）
    /// </summary>
    [HttpGet("heatmap")]
    public async Task<ActionResult<Dictionary<int, Dictionary<int, int>>>> GetUsageHeatmap([FromQuery] int days = 7)
    {
        try
        {
            if (days < 1 || days > 30)
                return BadRequest(new { error = "Days must be between 1 and 30" });

            var heatmapData = await _statisticsService.GetUsageHeatmapDataAsync(days);
            return Ok(heatmapData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取工具使用排行榜
    /// </summary>
    [HttpGet("ranking")]
    public async Task<ActionResult<List<ToolRankingItem>>> GetToolRanking([FromQuery] int topCount = 10, [FromQuery] int days = 30)
    {
        try
        {
            if (topCount < 1 || topCount > 50)
                return BadRequest(new { error = "TopCount must be between 1 and 50" });
            
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var ranking = await _statisticsService.GetToolRankingAsync(topCount, days);
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取所有统计数据（一次性获取，减少前端请求次数）
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardData([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var overview = await _statisticsService.GetOverviewStatisticsAsync();
            var trendData = await _statisticsService.GetUsageTrendDataAsync(days);
            var distribution = await _statisticsService.GetToolUsageDistributionAsync(days);
            var recentActivities = await _statisticsService.GetRecentActivitiesAsync(10);

            var dashboardData = new
            {
                overview,
                trendData,
                distribution,
                recentActivities,
                lastUpdated = DateTime.UtcNow
            };

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // =================== 客户端连接统计API接口 ===================

    /// <summary>
    /// 获取客户端连接概览统计
    /// </summary>
    [HttpGet("clients/overview")]
    public async Task<ActionResult<ClientOverviewStatistics>> GetClientOverviewStatistics()
    {
        try
        {
            var overview = await _statisticsService.GetClientOverviewStatisticsAsync();
            return Ok(overview);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端连接趋势数据
    /// </summary>
    [HttpGet("clients/trend")]
    public async Task<ActionResult<List<ClientTrendDataPoint>>> GetClientConnectionTrend([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var trendData = await _statisticsService.GetClientConnectionTrendAsync(days);
            return Ok(trendData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端分布数据
    /// </summary>
    [HttpGet("clients/distribution")]
    public async Task<ActionResult<List<ClientDistribution>>> GetClientDistribution([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var distribution = await _statisticsService.GetClientDistributionAsync(days);
            return Ok(distribution);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端版本分布数据
    /// </summary>
    [HttpGet("clients/version-distribution")]
    public async Task<ActionResult<List<ClientVersionDistribution>>> GetClientVersionDistribution([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var versionDistribution = await _statisticsService.GetClientVersionDistributionAsync(days);
            return Ok(versionDistribution);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取最近的客户端连接活动
    /// </summary>
    [HttpGet("clients/recent-activities")]
    public async Task<ActionResult<List<RecentClientActivity>>> GetRecentClientActivities([FromQuery] int count = 20)
    {
        try
        {
            if (count < 1 || count > 100)
                return BadRequest(new { error = "Count must be between 1 and 100" });

            var activities = await _statisticsService.GetRecentClientActivitiesAsync(count);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端连接排行榜
    /// </summary>
    [HttpGet("clients/ranking")]
    public async Task<ActionResult<List<ClientRankingItem>>> GetClientRanking([FromQuery] int topCount = 10, [FromQuery] int days = 30)
    {
        try
        {
            if (topCount < 1 || topCount > 50)
                return BadRequest(new { error = "TopCount must be between 1 and 50" });
            
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            var ranking = await _statisticsService.GetClientRankingAsync(topCount, days);
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端连接热力图数据
    /// </summary>
    [HttpGet("clients/heatmap")]
    public async Task<ActionResult<Dictionary<int, Dictionary<int, Dictionary<string, int>>>>> GetClientConnectionHeatmap([FromQuery] int days = 7)
    {
        try
        {
            if (days < 1 || days > 30)
                return BadRequest(new { error = "Days must be between 1 and 30" });

            var heatmapData = await _statisticsService.GetClientConnectionHeatmapAsync(days);
            return Ok(heatmapData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取包含客户端统计的完整仪表板数据
    /// </summary>
    [HttpGet("dashboard-with-clients")]
    public async Task<ActionResult<object>> GetDashboardWithClientsData([FromQuery] int days = 30)
    {
        try
        {
            if (days < 1 || days > 365)
                return BadRequest(new { error = "Days must be between 1 and 365" });

            // 工具统计
            var toolOverview = await _statisticsService.GetOverviewStatisticsAsync();
            var toolTrendData = await _statisticsService.GetUsageTrendDataAsync(days);
            var toolDistribution = await _statisticsService.GetToolUsageDistributionAsync(days);
            var toolRecentActivities = await _statisticsService.GetRecentActivitiesAsync(10);

            // 客户端统计
            var clientOverview = await _statisticsService.GetClientOverviewStatisticsAsync();
            var clientTrendData = await _statisticsService.GetClientConnectionTrendAsync(days);
            var clientDistribution = await _statisticsService.GetClientDistributionAsync(days);
            var clientVersionDistribution = await _statisticsService.GetClientVersionDistributionAsync(days);
            var clientRecentActivities = await _statisticsService.GetRecentClientActivitiesAsync(10);
            var clientRanking = await _statisticsService.GetClientRankingAsync(10, days);

            var dashboardData = new
            {
                tools = new
                {
                    overview = toolOverview,
                    trendData = toolTrendData,
                    distribution = toolDistribution,
                    recentActivities = toolRecentActivities
                },
                clients = new
                {
                    overview = clientOverview,
                    trendData = clientTrendData,
                    distribution = clientDistribution,
                    versionDistribution = clientVersionDistribution,
                    recentActivities = clientRecentActivities,
                    ranking = clientRanking
                },
                lastUpdated = DateTime.UtcNow
            };

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
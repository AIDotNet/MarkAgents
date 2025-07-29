// 统计数据类型定义
export interface OverviewStatistics {
  totalUsageCount: number
  activeToolsCount: number
  averageSuccessRate: number
  averageExecutionTime: number
  todayUsageCount: number
  todaySuccessRate: number
  uniqueSessions: number
  mostUsedTool: string
}

export interface TrendDataPoint {
  date: string
  totalUsage: number
  successCount: number
  failureCount: number
  successRate: number
  averageExecutionTime: number
}

export interface ToolUsageDistribution {
  toolName: string
  usageCount: number
  percentage: number
  color: string
}

export interface ToolSuccessRateStats {
  toolName: string
  totalUsageCount: number
  successCount: number
  failureCount: number
  successRate: number
}

export interface ToolPerformanceStats {
  toolName: string
  averageExecutionTime: number
  minExecutionTime: number
  maxExecutionTime: number
  medianExecutionTime: number
  totalCalls: number
}

export interface RecentActivityItem {
  toolName: string
  timestamp: string
  isSuccess: boolean
  executionTime: number
  sessionId?: string
  errorMessage?: string
}

export interface ToolRankingItem {
  rank: number
  toolName: string
  usageCount: number
  successRate: number
  averageExecutionTime: number
  trendChange: number
}

export interface DashboardData {
  overview: OverviewStatistics
  trendData: TrendDataPoint[]
  distribution: ToolUsageDistribution[]
  successRateStats: ToolSuccessRateStats[]
  performanceStats: ToolPerformanceStats[]
  recentActivities: RecentActivityItem[]
  ranking: ToolRankingItem[]
  lastUpdated: string
}

// 客户端统计数据类型定义
export interface ClientOverviewStatistics {
  totalConnections: number
  activeClientsCount: number
  averageConnectionSuccessRate: number
  averageConnectionDuration: number
  todayConnectionsCount: number
  todaySuccessRate: number
  uniqueSessionsToday: number
  mostActiveClient: string
}

export interface ClientTrendDataPoint {
  date: string
  clientConnections: Record<string, number>
  totalConnections: number
  successfulConnections: number
  failedConnections: number
  successRate: number
  averageConnectionDuration: number
}

export interface ClientDistribution {
  clientName: string
  connectionCount: number
  percentage: number
  color: string
}

export interface ClientVersionDistribution {
  clientName: string
  version: string
  connectionCount: number
  percentage: number
  color: string
}

export interface RecentClientActivity {
  clientName: string
  clientVersion?: string
  connectionTime: string
  status: 'Connected' | 'Disconnected' | 'Failed' | 'Timeout'
  connectionDuration?: number
  sessionId: string
  toolUsageCount: number
  errorMessage?: string
}

export interface ClientRankingItem {
  rank: number
  clientName: string
  clientVersion?: string
  connectionCount: number
  successRate: number
  averageConnectionDuration: number
  totalToolUsages: number
  trendChange: number
}

export interface CombinedDashboardData {
  tools: {
    overview: OverviewStatistics
    trendData: TrendDataPoint[]
    distribution: ToolUsageDistribution[]
    successRateStats: ToolSuccessRateStats[]
    recentActivities: RecentActivityItem[]
    ranking: ToolRankingItem[]
  }
  clients: {
    overview: ClientOverviewStatistics
    trendData: ClientTrendDataPoint[]
    distribution: ClientDistribution[]
    versionDistribution: ClientVersionDistribution[]
    recentActivities: RecentClientActivity[]
    ranking: ClientRankingItem[]
  }
  lastUpdated: string
}

// Channel统计数据类型定义
export interface ChannelStatistics {
  pendingEventCount: number
  totalProcessedEvents: number
  totalFailedEvents: number
  lastProcessedTime: string
  isHealthy: boolean
}

export interface ChannelHealthCheck {
  status: 'healthy' | 'unhealthy'
  timestamp: string
  details: {
    pendingEvents: number
    totalProcessed: number
    totalFailed: number
    lastProcessed: string
    successRate: number
  }
}


// API客户端类
export class StatisticsApi {
  private baseUrl: string

  constructor(baseUrl: string = "") {
    this.baseUrl = baseUrl
  }

  private async request<T>(endpoint: string): Promise<T> {
    try {
      const response = await fetch(`${this.baseUrl}/api/statistics${endpoint}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error(`API request failed for ${endpoint}:`, error)
      throw error
    }
  }

  // 获取概览统计数据
  async getOverviewStatistics(): Promise<OverviewStatistics> {
    return this.request<OverviewStatistics>('/overview')
  }

  // 获取使用趋势数据
  async getUsageTrend(days: number = 30): Promise<TrendDataPoint[]> {
    return this.request<TrendDataPoint[]>(`/trend?days=${days}`)
  }

  // 获取工具使用分布
  async getUsageDistribution(days: number = 30): Promise<ToolUsageDistribution[]> {
    return this.request<ToolUsageDistribution[]>(`/distribution?days=${days}`)
  }

  // 获取成功率统计
  async getSuccessRateStats(days: number = 30): Promise<ToolSuccessRateStats[]> {
    return this.request<ToolSuccessRateStats[]>(`/success-rate?days=${days}`)
  }

  // 获取性能统计
  async getPerformanceStats(days: number = 30): Promise<ToolPerformanceStats[]> {
    return this.request<ToolPerformanceStats[]>(`/performance?days=${days}`)
  }

  // 获取最近活动
  async getRecentActivities(count: number = 20): Promise<RecentActivityItem[]> {
    return this.request<RecentActivityItem[]>(`/recent-activities?count=${count}`)
  }

  // 获取使用热力图数据
  async getUsageHeatmap(days: number = 7): Promise<Record<number, Record<number, number>>> {
    return this.request<Record<number, Record<number, number>>>(`/heatmap?days=${days}`)
  }

  // 获取工具排行榜
  async getToolRanking(topCount: number = 10, days: number = 30): Promise<ToolRankingItem[]> {
    return this.request<ToolRankingItem[]>(`/ranking?topCount=${topCount}&days=${days}`)
  }

  // 获取仪表板所有数据（推荐使用，减少请求次数）
  async getDashboardData(days: number = 30): Promise<DashboardData> {
    return this.request<DashboardData>(`/dashboard?days=${days}`)
  }

  // =================== 客户端统计相关API方法 ===================

  // 获取客户端连接概览统计
  async getClientOverviewStatistics(): Promise<ClientOverviewStatistics> {
    return this.request<ClientOverviewStatistics>('/clients/overview')
  }

  // 获取客户端连接趋势
  async getClientConnectionTrend(days: number = 30): Promise<ClientTrendDataPoint[]> {
    return this.request<ClientTrendDataPoint[]>(`/clients/trend?days=${days}`)
  }

  // 获取客户端连接分布
  async getClientDistribution(days: number = 30): Promise<ClientDistribution[]> {
    return this.request<ClientDistribution[]>(`/clients/distribution?days=${days}`)
  }

  // 获取客户端版本分布
  async getClientVersionDistribution(days: number = 30): Promise<ClientVersionDistribution[]> {
    return this.request<ClientVersionDistribution[]>(`/clients/version-distribution?days=${days}`)
  }

  // 获取最近客户端活动
  async getRecentClientActivities(count: number = 20): Promise<RecentClientActivity[]> {
    return this.request<RecentClientActivity[]>(`/clients/recent-activities?count=${count}`)
  }

  // 获取客户端排行榜
  async getClientRanking(topCount: number = 10, days: number = 30): Promise<ClientRankingItem[]> {
    return this.request<ClientRankingItem[]>(`/clients/ranking?topCount=${topCount}&days=${days}`)
  }

  // 获取客户端连接热力图数据
  async getClientConnectionHeatmap(days: number = 7): Promise<Record<number, Record<number, Record<string, number>>>> {
    return this.request<Record<number, Record<number, Record<string, number>>>>(`/clients/heatmap?days=${days}`)
  }

  // 获取包含客户端统计的完整仪表板数据（推荐使用）
  async getCombinedDashboardData(days: number = 30): Promise<CombinedDashboardData> {
    return this.request<CombinedDashboardData>(`/dashboard-with-clients?days=${days}`)
  }

  // =================== Channel监控相关API方法 ===================

  // 获取Channel状态
  async getChannelStatus(): Promise<ChannelStatistics> {
    return this.request<ChannelStatistics>('/channel/status')
  }

  // Channel健康检查
  async getChannelHealth(): Promise<ChannelHealthCheck> {
    return this.request<ChannelHealthCheck>('/channel/health')
  }
}

// 创建默认实例
export const statisticsApi = new StatisticsApi()

// React Hook for statistics data
export function useStatistics() {
  const [data, setData] = React.useState<DashboardData | null>(null)
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState<string | null>(null)

  const fetchDashboardData = React.useCallback(async (days: number = 30) => {
    setLoading(true)
    setError(null)
    
    try {
      const dashboardData = await statisticsApi.getDashboardData(days)
      setData(dashboardData)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : '获取统计数据失败'
      setError(errorMessage)
      console.error('Failed to fetch dashboard data:', err)
    } finally {
      setLoading(false)
    }
  }, [])

  return {
    data,
    loading,
    error,
    fetchDashboardData,
    refetch: () => fetchDashboardData(30)
  }
}

// React Hook for combined statistics data (包括客户端统计)
export function useCombinedStatistics() {
  const [data, setData] = React.useState<CombinedDashboardData | null>(null)
  const [loading, setLoading] = React.useState(false)
  const [error, setError] = React.useState<string | null>(null)

  const fetchCombinedDashboardData = React.useCallback(async (days: number = 30) => {
    setLoading(true)
    setError(null)
    
    try {
      const dashboardData = await statisticsApi.getCombinedDashboardData(days)
      setData(dashboardData)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : '获取统计数据失败'
      setError(errorMessage)
      console.error('Failed to fetch combined dashboard data:', err)
    } finally {
      setLoading(false)
    }
  }, [])

  return {
    data,
    loading,
    error,
    fetchCombinedDashboardData,
    refetch: () => fetchCombinedDashboardData(30)
  }
}

// 为兼容性导入React
import * as React from "react"
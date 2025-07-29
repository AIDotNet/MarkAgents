"use client"

import * as React from "react"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { RefreshCw, AlertCircle, Activity, Laptop } from "lucide-react"
import { useCombinedStatistics } from "@/lib/statistics-api"
import { StatisticsOverviewCards } from "./statistics-overview-cards"
import { ToolUsageTrendChart } from "./tool-usage-trend-chart"
import { ToolUsageDistributionChart } from "./tool-usage-distribution-chart"
import { RecentActivities } from "./recent-activities"
import { ClientOverviewCards } from "./client-overview-cards"
import { ClientConnectionTrendChart } from "./client-connection-trend-chart"
import { ClientDistributionChart } from "./client-distribution-chart"
import { RecentClientActivities } from "./recent-client-activities"
import { ClientRanking } from "./client-ranking"
import { ChannelStatusCard } from "./channel-status-card"

export function StatisticsDashboard() {
  const { data, loading, error, fetchCombinedDashboardData, refetch } = useCombinedStatistics()

  React.useEffect(() => {
    // 组件挂载时获取数据
    fetchCombinedDashboardData(30)
  }, [fetchCombinedDashboardData])

  // 错误状态
  if (error && !data) {
    return (
      <Card className="col-span-full">
        <CardContent className="flex flex-col items-center justify-center h-[400px]">
          <AlertCircle className="h-12 w-12 text-red-500 mb-4" />
          <h3 className="text-lg font-semibold mb-2">数据加载失败</h3>
          <p className="text-muted-foreground mb-4 text-center">
            {error}
          </p>
          <Button onClick={refetch} variant="outline">
            <RefreshCw className="h-4 w-4 mr-2" />
            重试
          </Button>
        </CardContent>
      </Card>
    )
  }

  // 空数据状态（但没有错误）
  if (!loading && !data) {
    return (
      <Card className="col-span-full">
        <CardContent className="flex flex-col items-center justify-center h-[400px]">
          <div className="text-center">
            <h3 className="text-lg font-semibold mb-2">暂无统计数据</h3>
            <p className="text-muted-foreground mb-4">
              请先使用一些工具功能，然后数据将会显示在这里
            </p>
            <Button onClick={refetch} variant="outline">
              <RefreshCw className="h-4 w-4 mr-2" />
              刷新
            </Button>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      {/* 刷新按钮 */}
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-2xl font-bold">AgentTools 使用统计</h2>
          <p className="text-muted-foreground">
            实时监控工具使用情况、客户端连接和性能指标
          </p>
        </div>
        <Button 
          onClick={refetch} 
          variant="outline" 
          size="sm"
          disabled={loading}
        >
          <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
          刷新数据
        </Button>
      </div>

      {/* 统计标签页 */}
      <Tabs defaultValue="tools" className="w-full">
        <TabsList className="grid w-full grid-cols-2 mb-6">
          <TabsTrigger value="tools" className="flex items-center gap-2">
            <Activity className="h-4 w-4" />
            工具统计
          </TabsTrigger>
          <TabsTrigger value="clients" className="flex items-center gap-2">
            <Laptop className="h-4 w-4" />
            客户端统计
          </TabsTrigger>
        </TabsList>

        {/* 工具统计标签页 */}
        <TabsContent value="tools" className="space-y-6">
          {/* 工具概览卡片 */}
          <div className="grid gap-6 md:grid-cols-4">
            <div className="md:col-span-3">
              <StatisticsOverviewCards 
                data={data?.tools?.overview || {
                  totalUsageCount: 0,
                  activeToolsCount: 0,
                  averageSuccessRate: 0,
                  averageExecutionTime: 0,
                  todayUsageCount: 0,
                  todaySuccessRate: 0,
                  uniqueSessions: 0,
                  mostUsedTool: '无'
                }} 
                loading={loading}
              />
            </div>
            <div className="md:col-span-1">
              <ChannelStatusCard />
            </div>
          </div>

          {/* 工具图表区域 */}
          <div className="grid gap-6 md:grid-cols-2">
            {/* 使用趋势图 */}
            <div className="md:col-span-2">
              <ToolUsageTrendChart 
                data={data?.tools?.trendData || []} 
                loading={loading}
              />
            </div>

            {/* 工具分布饼图 */}
            <ToolUsageDistributionChart 
              data={data?.tools?.distribution || []} 
              loading={loading}
            />

            {/* 最近活动 */}
            <RecentActivities 
              data={data?.tools?.recentActivities || []} 
              loading={loading}
            />
          </div>
        </TabsContent>

        {/* 客户端统计标签页 */}
        <TabsContent value="clients" className="space-y-6">
          {/* 客户端统计说明 */}
          <div className="bg-blue-50 dark:bg-blue-950/20 p-4 rounded-lg border border-blue-200 dark:border-blue-800">
            <div className="flex items-start gap-3">
              <div className="h-5 w-5 bg-blue-500 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                <span className="text-white text-xs font-bold">i</span>
              </div>
              <div>
                <h4 className="text-sm font-medium text-blue-800 dark:text-blue-200 mb-1">
                  客户端统计说明
                </h4>
                <p className="text-sm text-blue-700 dark:text-blue-300">
                  仅统计带有有效UserAgent的客户端连接，确保数据的准确性和可靠性。
                  无UserAgent的连接不会被纳入统计分析。
                </p>
              </div>
            </div>
          </div>

          {/* 客户端概览卡片 */}
          <ClientOverviewCards 
            data={data?.clients?.overview || {
              totalConnections: 0,
              activeClientsCount: 0,
              averageConnectionSuccessRate: 0,
              averageConnectionDuration: 0,
              todayConnectionsCount: 0,
              todaySuccessRate: 0,
              uniqueSessionsToday: 0,
              mostActiveClient: '无'
            }} 
            loading={loading}
          />

          {/* 客户端图表区域 */}
          <div className="grid gap-6 md:grid-cols-2">
            {/* 连接趋势图 */}
            <div className="md:col-span-2">
              <ClientConnectionTrendChart 
                data={data?.clients?.trendData || []} 
                loading={loading}
              />
            </div>

            {/* 客户端分布饼图 */}
            <ClientDistributionChart 
              data={data?.clients?.distribution || []} 
              loading={loading}
            />

            {/* 版本分布饼图 */}
            <ClientDistributionChart 
              data={data?.clients?.versionDistribution?.map(item => ({
                clientName: `${item.clientName} v${item.version}`,
                connectionCount: item.connectionCount,
                percentage: item.percentage,
                color: item.color
              })) || []} 
              loading={loading}
            />
          </div>

          {/* 客户端活动和排行榜 */}
          <div className="grid gap-6 md:grid-cols-2">
            {/* 最近连接活动 */}
            <RecentClientActivities 
              data={data?.clients?.recentActivities || []} 
              loading={loading}
            />

            {/* 客户端排行榜 */}
            <ClientRanking 
              data={data?.clients?.ranking || []} 
              loading={loading}
            />
          </div>
        </TabsContent>
      </Tabs>

      {/* 数据更新时间 */}
      {data?.lastUpdated && (
        <div className="text-center text-sm text-muted-foreground">
          数据更新时间: {new Date(data.lastUpdated).toLocaleString('zh-CN')}
        </div>
      )}
    </div>
  )
}
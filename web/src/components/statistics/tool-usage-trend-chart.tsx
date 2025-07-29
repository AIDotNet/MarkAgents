"use client"

import * as React from "react"
import { Area, AreaChart, CartesianGrid, XAxis, YAxis, ResponsiveContainer, Tooltip } from "recharts"
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  ToggleGroup,
  ToggleGroupItem,
} from "@/components/ui/toggle-group"

import { useIsMobile } from "@/hooks/use-mobile"

interface TrendDataPoint {
  date: string
  totalUsage: number
  successCount: number
  failureCount: number
  successRate: number
  averageExecutionTime: number
}

interface ToolUsageTrendChartProps {
  data: TrendDataPoint[]
  loading?: boolean
}

export function ToolUsageTrendChart({ data, loading = false }: ToolUsageTrendChartProps) {
  const isMobile = useIsMobile()
  const [timeRange, setTimeRange] = React.useState("30d")
  const [metric, setMetric] = React.useState<"usage" | "successRate" | "executionTime">("usage")

  const filteredData = React.useMemo(() => {
    if (!data.length) return []
    
    const days = timeRange === "7d" ? 7 : timeRange === "30d" ? 30 : 90
    return data.slice(-days).map(item => ({
      ...item,
      date: new Date(item.date).toLocaleDateString("zh-CN", {
        month: "short",
        day: "numeric",
      })
    }))
  }, [data, timeRange])

  const getMetricData = () => {
    switch (metric) {
      case "successRate":
        return {
          dataKey: "successRate",
          label: "成功率 (%)",
          color: "var(--chart-2)",
          gradientId: "fillSuccessRate"
        }
      case "executionTime":
        return {
          dataKey: "averageExecutionTime",
          label: "平均执行时间 (ms)",
          color: "var(--chart-3)",
          gradientId: "fillExecutionTime"
        }
      default:
        return {
          dataKey: "totalUsage",
          label: "使用次数",
          color: "var(--primary)",
          gradientId: "fillUsage"
        }
    }
  }

  const metricConfig = getMetricData()

  if (loading) {
    return (
      <Card className="animate-pulse">
        <CardHeader>
          <div className="h-6 bg-gray-200 rounded w-1/3"></div>
          <div className="h-4 bg-gray-200 rounded w-1/2 mt-2"></div>
        </CardHeader>
        <CardContent>
          <div className="h-[300px] bg-gray-200 rounded"></div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="@container/chart">
      <CardHeader>
        <CardTitle>工具使用趋势</CardTitle>
        <CardDescription>
          <span className="hidden @[540px]/chart:block">
            查看工具使用的趋势变化
          </span>
          <span className="@[540px]/chart:hidden">使用趋势</span>
        </CardDescription>
        <CardAction>
          <div className="flex flex-col gap-2 @[767px]/chart:flex-row @[767px]/chart:items-center">
            {/* 指标选择 */}
            <Select value={metric} onValueChange={(value: any) => setMetric(value)}>
              <SelectTrigger className="w-40">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="usage">使用次数</SelectItem>
                <SelectItem value="successRate">成功率</SelectItem>
                <SelectItem value="executionTime">执行时间</SelectItem>
              </SelectContent>
            </Select>

            {/* 时间范围选择 - 桌面端 */}
            <ToggleGroup
              type="single"
              value={timeRange}
              onValueChange={setTimeRange}
              variant="outline"
              className="hidden @[767px]/chart:flex"
            >
              <ToggleGroupItem value="7d">7天</ToggleGroupItem>
              <ToggleGroupItem value="30d">30天</ToggleGroupItem>
              <ToggleGroupItem value="90d">90天</ToggleGroupItem>
            </ToggleGroup>

            {/* 时间范围选择 - 移动端 */}
            <Select value={timeRange} onValueChange={setTimeRange}>
              <SelectTrigger className="w-20 @[767px]/chart:hidden">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="7d">7天</SelectItem>
                <SelectItem value="30d">30天</SelectItem>
                <SelectItem value="90d">90天</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardAction>
      </CardHeader>
      <CardContent className="px-2 pt-4 sm:px-6 sm:pt-6">
        <div className="aspect-auto h-[300px] w-full">
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={filteredData}>
              <defs>
                <linearGradient id={metricConfig.gradientId} x1="0" y1="0" x2="0" y2="1">
                  <stop
                    offset="5%"
                    stopColor={metricConfig.color}
                    stopOpacity={0.8}
                  />
                  <stop
                    offset="95%"
                    stopColor={metricConfig.color}
                    stopOpacity={0.1}
                  />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis
                dataKey="date"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                minTickGap={32}
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                minTickGap={32}
              />
              <Tooltip
                content={({ active, payload, label }) => {
                  if (active && payload && payload.length) {
                    return (
                      <div className="rounded-lg border bg-background p-2 shadow-sm">
                        <div className="grid grid-cols-2 gap-2">
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              日期
                            </span>
                            <span className="font-bold text-muted-foreground">
                              {label}
                            </span>
                          </div>
                          <div className="flex flex-col">
                            <span className="text-[0.70rem] uppercase text-muted-foreground">
                              {metricConfig.label}
                            </span>
                            <span className="font-bold">
                              {metric === "successRate" 
                                ? `${payload[0].value}%`
                                : metric === "executionTime"
                                ? `${payload[0].value}ms`
                                : payload[0].value
                              }
                            </span>
                          </div>
                        </div>
                      </div>
                    )
                  }
                  return null
                }}
              />
              <Area
                dataKey={metricConfig.dataKey}
                type="natural"
                fill={`url(#${metricConfig.gradientId})`}
                stroke={metricConfig.color}
                strokeWidth={2}
              />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  )
}
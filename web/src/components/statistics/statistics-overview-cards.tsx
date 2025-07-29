import { Card, CardContent,  CardHeader, CardTitle } from "@/components/ui/card"
import { 
  Bot, 
  Activity, 
  CheckCircle, 
  Clock,
  TrendingUp,
  TrendingDown,
  Minus
} from "lucide-react"

interface OverviewStatistics {
  totalUsageCount: number
  activeToolsCount: number
  averageSuccessRate: number
  averageExecutionTime: number
  todayUsageCount: number
  todaySuccessRate: number
  uniqueSessions: number
  mostUsedTool: string
}

interface StatisticsOverviewCardsProps {
  data: OverviewStatistics
  loading?: boolean
}

export function StatisticsOverviewCards({ data, loading = false }: StatisticsOverviewCardsProps) {
  if (loading) {
    return (
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Card key={i} className="animate-pulse">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <div className="h-4 bg-gray-200 rounded w-1/2"></div>
              <div className="h-4 w-4 bg-gray-200 rounded"></div>
            </CardHeader>
            <CardContent>
              <div className="h-8 bg-gray-200 rounded w-1/3 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-2/3"></div>
            </CardContent>
          </Card>
        ))}
      </div>
    )
  }

  const getTrendIcon = (current: number, average: number) => {
    if (current > average * 1.1) return <TrendingUp className="h-4 w-4 text-green-600" />
    if (current < average * 0.9) return <TrendingDown className="h-4 w-4 text-red-600" />
    return <Minus className="h-4 w-4 text-gray-600" />
  }

  const formatExecutionTime = (ms: number) => {
    if (ms < 1000) return `${Math.round(ms)}ms`
    return `${(ms / 1000).toFixed(1)}s`
  }

  const stats = [
    {
      title: "总使用次数",
      value: data.totalUsageCount.toLocaleString(),
      change: data.todayUsageCount > 0 ? `今日 ${data.todayUsageCount}` : "今日无使用",
      trend: getTrendIcon(data.todayUsageCount, data.totalUsageCount / 30),
      icon: Activity,
      description: "工具总调用次数"
    },
    {
      title: "活跃工具数",
      value: data.activeToolsCount.toString(),
      change: `最常用: ${data.mostUsedTool}`,
      trend: <Bot className="h-4 w-4 text-blue-600" />,
      icon: Bot,
      description: "近30天使用的工具"
    },
    {
      title: "平均成功率",
      value: `${data.averageSuccessRate.toFixed(1)}%`,
      change: `今日 ${data.todaySuccessRate.toFixed(1)}%`,
      trend: getTrendIcon(data.todaySuccessRate, data.averageSuccessRate),
      icon: CheckCircle,
      description: "近7天成功率统计"
    },
    {
      title: "平均执行时间",
      value: formatExecutionTime(data.averageExecutionTime),
      change: `${data.uniqueSessions} 个会话`,
      trend: <Clock className="h-4 w-4 text-purple-600" />,
      icon: Clock,
      description: "近7天平均耗时"
    }
  ]

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {stats.map((stat, index) => (
        <Card key={index} className="hover:shadow-lg transition-shadow">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              {stat.title}
            </CardTitle>
            <stat.icon className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stat.value}</div>
            <div className="flex items-center text-xs text-muted-foreground mt-1">
              {stat.trend}
              <span className="ml-1">{stat.change}</span>
            </div>
            <p className="text-xs text-muted-foreground mt-1">
              {stat.description}
            </p>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}
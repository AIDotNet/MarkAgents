import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { 
  Laptop, 
  Users, 
  CheckCircle, 
  Clock,
  TrendingUp,
  TrendingDown,
  Minus
} from "lucide-react"

interface ClientOverviewStatistics {
  totalConnections: number
  activeClientsCount: number
  averageConnectionSuccessRate: number
  averageConnectionDuration: number
  todayConnectionsCount: number
  todaySuccessRate: number
  uniqueSessionsToday: number
  mostActiveClient: string
}

interface ClientOverviewCardsProps {
  data: ClientOverviewStatistics
  loading?: boolean
}

export function ClientOverviewCards({ data, loading = false }: ClientOverviewCardsProps) {
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

  const formatDuration = (seconds: number) => {
    if (seconds < 60) return `${Math.round(seconds)}秒`
    if (seconds < 3600) return `${Math.round(seconds / 60)}分钟`
    return `${(seconds / 3600).toFixed(1)}小时`
  }

  const stats = [
    {
      title: "总连接次数",
      value: data.totalConnections.toLocaleString(),
      change: data.todayConnectionsCount > 0 ? `今日 ${data.todayConnectionsCount}` : "今日无连接",
      trend: getTrendIcon(data.todayConnectionsCount, data.totalConnections / 30),
      icon: Laptop,
      description: "客户端总连接次数"
    },
    {
      title: "活跃客户端",
      value: data.activeClientsCount.toString(),
      change: `主要客户端: ${data.mostActiveClient}`,
      trend: <Users className="h-4 w-4 text-blue-600" />,
      icon: Users,
      description: "近30天活跃客户端数"
    },
    {
      title: "平均成功率",
      value: `${data.averageConnectionSuccessRate.toFixed(1)}%`,
      change: `今日 ${data.todaySuccessRate.toFixed(1)}%`,
      trend: getTrendIcon(data.todaySuccessRate, data.averageConnectionSuccessRate),
      icon: CheckCircle,
      description: "近7天连接成功率"
    },
    {
      title: "平均连接时长",
      value: formatDuration(data.averageConnectionDuration),
      change: `${data.uniqueSessionsToday} 个会话`,
      trend: <Clock className="h-4 w-4 text-purple-600" />,
      icon: Clock,
      description: "近7天平均连接时长"
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
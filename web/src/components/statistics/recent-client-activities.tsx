import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { 
  Laptop, 
  CheckCircle, 
  XCircle, 
  Clock,
  AlertTriangle,
  Wifi,
  WifiOff
} from "lucide-react"

interface RecentClientActivity {
  clientName: string
  clientVersion?: string
  connectionTime: string
  status: 'Connected' | 'Disconnected' | 'Failed' | 'Timeout'
  connectionDuration?: number
  sessionId: string
  toolUsageCount: number
  errorMessage?: string
}

interface RecentClientActivitiesProps {
  data: RecentClientActivity[]
  loading?: boolean
}

export function RecentClientActivities({ data, loading = false }: RecentClientActivitiesProps) {
  if (loading) {
    return (
      <Card className="animate-pulse">
        <CardHeader>
          <div className="h-6 bg-gray-200 rounded w-1/3"></div>
          <div className="h-4 bg-gray-200 rounded w-1/2 mt-2"></div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex items-center gap-4">
                <div className="w-8 h-8 bg-gray-200 rounded-full"></div>
                <div className="flex-1">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    )
  }

  const formatRelativeTime = (timestamp: string) => {
    const date = new Date(timestamp)
    const now = new Date()
    const diffMs = now.getTime() - date.getTime()
    
    const diffMinutes = Math.floor(diffMs / (1000 * 60))
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
    
    if (diffMinutes < 1) return "刚刚"
    if (diffMinutes < 60) return `${diffMinutes}分钟前`
    if (diffHours < 24) return `${diffHours}小时前`
    if (diffDays < 7) return `${diffDays}天前`
    
    return date.toLocaleDateString("zh-CN", {
      month: "short",
      day: "numeric",
    })
  }

  const formatDuration = (seconds?: number) => {
    if (!seconds) return "连接中"
    if (seconds < 60) return `${Math.round(seconds)}秒`
    if (seconds < 3600) return `${Math.round(seconds / 60)}分钟`
    return `${(seconds / 3600).toFixed(1)}小时`
  }

  const getActivityIcon = (activity: RecentClientActivity) => {
    switch (activity.status) {
      case 'Connected':
        return <Wifi className="h-4 w-4 text-green-600" />
      case 'Disconnected':
        return <CheckCircle className="h-4 w-4 text-blue-600" />
      case 'Failed':
        return <XCircle className="h-4 w-4 text-red-600" />
      case 'Timeout':
        return <WifiOff className="h-4 w-4 text-orange-600" />
      default:
        return <Laptop className="h-4 w-4 text-gray-600" />
    }
  }

  const getActivityBadge = (activity: RecentClientActivity) => {
    switch (activity.status) {
      case 'Connected':
        return (
          <Badge variant="secondary" className="bg-green-100 text-green-800 hover:bg-green-100">
            已连接
          </Badge>
        )
      case 'Disconnected':
        return (
          <Badge variant="secondary" className="bg-blue-100 text-blue-800 hover:bg-blue-100">
            已断开
          </Badge>
        )
      case 'Failed':
        return (
          <Badge variant="secondary" className="bg-red-100 text-red-800 hover:bg-red-100">
            连接失败
          </Badge>
        )
      case 'Timeout':
        return (
          <Badge variant="secondary" className="bg-orange-100 text-orange-800 hover:bg-orange-100">
            超时
          </Badge>
        )
      default:
        return (
          <Badge variant="secondary">
            未知
          </Badge>
        )
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Laptop className="h-5 w-5" />
          客户端连接活动
        </CardTitle>
        <CardDescription>
          最近的客户端连接记录
        </CardDescription>
      </CardHeader>
      <CardContent>
        {!data.length ? (
          <div className="flex items-center justify-center h-[200px] text-muted-foreground">
            暂无连接记录
          </div>
        ) : (
          <ScrollArea className="h-[400px] pr-4">
            <div className="space-y-4">
              {data.map((activity, index) => (
                <div key={index} className="flex items-start gap-4 pb-4 border-b border-border/50 last:border-0">
                  <div className="flex-shrink-0 mt-1">
                    {getActivityIcon(activity)}
                  </div>
                  
                  <div className="flex-1 min-w-0 space-y-2">
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-sm">
                          {activity.clientName}
                        </span>
                        {activity.clientVersion && (
                          <span className="text-xs text-muted-foreground font-mono">
                            v{activity.clientVersion}
                          </span>
                        )}
                        {getActivityBadge(activity)}
                      </div>
                      <div className="flex items-center gap-2 text-xs text-muted-foreground">
                        <Clock className="h-3 w-3" />
                        {formatDuration(activity.connectionDuration)}
                      </div>
                    </div>
                    
                    <div className="grid grid-cols-2 gap-4 text-xs text-muted-foreground">
                      <div>
                        <span className="font-medium">连接时间:</span>
                        <br />
                        {formatRelativeTime(activity.connectionTime)}
                      </div>
                      <div>
                        <span className="font-medium">工具使用:</span>
                        <br />
                        {activity.toolUsageCount} 次
                      </div>
                    </div>
                    
                    <div className="text-xs text-muted-foreground font-mono">
                      会话: {activity.sessionId.slice(-12)}
                    </div>
                    
                    {activity.errorMessage && (
                      <div className="flex items-start gap-2 p-2 bg-red-50 dark:bg-red-950/20 rounded text-xs">
                        <AlertTriangle className="h-3 w-3 text-red-600 mt-0.5 flex-shrink-0" />
                        <span className="text-red-700 dark:text-red-400 break-words">
                          {activity.errorMessage}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </ScrollArea>
        )}
      </CardContent>
    </Card>
  )
}
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { 
  CheckCircle, 
  XCircle, 
  Clock,
  Bot,
  AlertTriangle
} from "lucide-react"

interface RecentActivityItem {
  toolName: string
  timestamp: string
  isSuccess: boolean
  executionTime: number
  sessionId?: string
  errorMessage?: string
}

interface RecentActivitiesProps {
  data: RecentActivityItem[]
  loading?: boolean
}

export function RecentActivities({ data, loading = false }: RecentActivitiesProps) {
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

  const formatExecutionTime = (ms: number) => {
    if (ms < 1000) return `${Math.round(ms)}ms`
    return `${(ms / 1000).toFixed(1)}s`
  }

  const getActivityIcon = (activity: RecentActivityItem) => {
    if (activity.isSuccess) {
      return <CheckCircle className="h-4 w-4 text-green-600" />
    } else {
      return <XCircle className="h-4 w-4 text-red-600" />
    }
  }

  const getActivityBadge = (activity: RecentActivityItem) => {
    if (activity.isSuccess) {
      return (
        <Badge variant="secondary" className="bg-green-100 text-green-800 hover:bg-green-100">
          成功
        </Badge>
      )
    } else {
      return (
        <Badge variant="secondary" className="bg-red-100 text-red-800 hover:bg-red-100">
          失败
        </Badge>
      )
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Bot className="h-5 w-5" />
          最近活动
        </CardTitle>
        <CardDescription>
          工具执行的最新记录
        </CardDescription>
      </CardHeader>
      <CardContent>
        {!data.length ? (
          <div className="flex items-center justify-center h-[200px] text-muted-foreground">
            暂无活动记录
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
                          {activity.toolName}
                        </span>
                        {getActivityBadge(activity)}
                      </div>
                      <div className="flex items-center gap-2 text-xs text-muted-foreground">
                        <Clock className="h-3 w-3" />
                        {formatExecutionTime(activity.executionTime)}
                      </div>
                    </div>
                    
                    <div className="flex items-center justify-between text-xs text-muted-foreground">
                      <span>{formatRelativeTime(activity.timestamp)}</span>
                      {activity.sessionId && (
                        <span className="font-mono">
                          会话: {activity.sessionId.slice(-8)}
                        </span>
                      )}
                    </div>
                    
                    {!activity.isSuccess && activity.errorMessage && (
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
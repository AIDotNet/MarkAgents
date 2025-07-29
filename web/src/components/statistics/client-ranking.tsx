import * as React from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { 
  Trophy,
  Medal,
  Award,
  Laptop,
  TrendingUp,
  TrendingDown,
  Minus,
  Clock,
  Zap
} from "lucide-react"

interface ClientRankingItem {
  rank: number
  clientName: string
  clientVersion?: string
  connectionCount: number
  successRate: number
  averageConnectionDuration: number
  totalToolUsages: number
  trendChange: number
}

interface ClientRankingProps {
  data: ClientRankingItem[]
  loading?: boolean
}

export function ClientRanking({ data, loading = false }: ClientRankingProps) {
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

  const getRankIcon = (rank: number) => {
    switch (rank) {
      case 1:
        return <Trophy className="h-5 w-5 text-yellow-500" />
      case 2:
        return <Medal className="h-5 w-5 text-gray-400" />
      case 3:
        return <Award className="h-5 w-5 text-amber-600" />
      default:
        return (
          <div className="h-5 w-5 rounded-full bg-muted flex items-center justify-center text-xs font-medium">
            {rank}
          </div>
        )
    }
  }

  const getTrendIcon = (change: number) => {
    if (change > 0) return <TrendingUp className="h-3 w-3 text-green-600" />
    if (change < 0) return <TrendingDown className="h-3 w-3 text-red-600" />
    return <Minus className="h-3 w-3 text-gray-600" />
  }

  const getTrendText = (change: number) => {
    if (change > 0) return `+${change}`
    return change.toString()
  }

  const getSuccessRateBadge = (rate: number) => {
    if (rate >= 95) {
      return (
        <Badge variant="secondary" className="bg-green-100 text-green-800 hover:bg-green-100">
          优秀
        </Badge>
      )
    } else if (rate >= 80) {
      return (
        <Badge variant="secondary" className="bg-yellow-100 text-yellow-800 hover:bg-yellow-100">
          良好
        </Badge>
      )
    } else {
      return (
        <Badge variant="secondary" className="bg-red-100 text-red-800 hover:bg-red-100">
          待优化
        </Badge>
      )
    }
  }

  const formatDuration = (seconds: number) => {
    if (seconds < 60) return `${Math.round(seconds)}秒`
    if (seconds < 3600) return `${Math.round(seconds / 60)}分钟`
    return `${(seconds / 3600).toFixed(1)}小时`
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Trophy className="h-5 w-5" />
          客户端活跃度排行
        </CardTitle>
        <CardDescription>
          按连接次数排序的客户端排行
        </CardDescription>
      </CardHeader>
      <CardContent>
        {!data.length ? (
          <div className="flex items-center justify-center h-[300px] text-muted-foreground">
            暂无排行数据
          </div>
        ) : (
          <ScrollArea className="h-[400px] pr-4">
            <div className="space-y-3">
              {data.map((item, index) => (
                <div 
                  key={index} 
                  className={`flex items-center gap-4 p-3 rounded-lg border transition-colors hover:bg-muted/50 ${
                    item.rank <= 3 ? 'bg-muted/20' : ''
                  }`}
                >
                  {/* 排名图标 */}
                  <div className="flex-shrink-0">
                    {getRankIcon(item.rank)}
                  </div>
                  
                  {/* 客户端信息 */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <h4 className="font-medium text-sm">{item.clientName}</h4>
                        {item.clientVersion && (
                          <span className="text-xs text-muted-foreground font-mono">
                            v{item.clientVersion}
                          </span>
                        )}
                      </div>
                      <div className="flex items-center gap-1 text-xs text-muted-foreground">
                        {getTrendIcon(item.trendChange)}
                        <span>{getTrendText(item.trendChange)}</span>
                      </div>
                    </div>
                    
                    <div className="grid grid-cols-2 gap-4 text-xs text-muted-foreground mb-2">
                      <div>
                        <span className="font-medium text-foreground">
                          {item.connectionCount.toLocaleString()}
                        </span>
                        <span className="ml-1">次连接</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Clock className="h-3 w-3" />
                        <span>{formatDuration(item.averageConnectionDuration)}</span>
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4 text-xs text-muted-foreground mb-2">
                      <div className="flex items-center gap-1">
                        <Zap className="h-3 w-3" />
                        <span>{item.totalToolUsages} 次工具使用</span>
                      </div>
                      <div>
                        成功率: {item.successRate.toFixed(1)}%
                      </div>
                    </div>
                    
                    <div className="flex items-center justify-between">
                      <div className="text-xs text-muted-foreground">
                        平均每连接工具使用: {item.connectionCount > 0 ? (item.totalToolUsages / item.connectionCount).toFixed(1) : '0'} 次
                      </div>
                      {getSuccessRateBadge(item.successRate)}
                    </div>
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
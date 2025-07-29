import { useEffect, useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { RefreshCw, Activity, AlertCircle, CheckCircle, Clock, TrendingUp } from 'lucide-react'
import { statisticsApi, type ChannelHealthCheck } from '@/lib/statistics-api'

export function ChannelStatusCard() {
  const [healthData, setHealthData] = useState<ChannelHealthCheck | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchChannelHealth = async () => {
    setLoading(true)
    setError(null)
    
    try {
      const health = await statisticsApi.getChannelHealth()
      setHealthData(health)
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : '获取Channel状态失败'
      setError(errorMessage)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchChannelHealth()
    
    // 每30秒自动刷新
    const interval = setInterval(fetchChannelHealth, 30000)
    return () => clearInterval(interval)
  }, [])

  if (error) {
    return (
      <Card className="border-red-200 bg-red-50/50 dark:border-red-800 dark:bg-red-950/50">
        <CardContent className="flex items-center justify-center h-48">
          <div className="text-center">
            <AlertCircle className="h-8 w-8 text-red-500 mx-auto mb-2" />
            <p className="text-sm text-red-600 dark:text-red-400 mb-3">{error}</p>
            <Button size="sm" variant="outline" onClick={fetchChannelHealth}>
              <RefreshCw className="h-4 w-4 mr-2" />
              重试
            </Button>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <div>
          <CardTitle className="text-sm font-medium flex items-center gap-2">
            <Activity className="h-4 w-4" />
            Channel 状态监控
          </CardTitle>
          <CardDescription>
            统计数据处理队列状态
          </CardDescription>
        </div>
        <div className="flex items-center gap-2">
          {healthData && (
            <Badge variant={healthData.status === 'healthy' ? 'default' : 'destructive'}>
              {healthData.status === 'healthy' ? (
                <CheckCircle className="h-3 w-3 mr-1" />
              ) : (
                <AlertCircle className="h-3 w-3 mr-1" />
              )}
              {healthData.status === 'healthy' ? '健康' : '异常'}
            </Badge>
          )}
          <Button size="sm" variant="ghost" onClick={fetchChannelHealth} disabled={loading}>
            <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </CardHeader>

      <CardContent>
        {loading ? (
          <div className="flex items-center justify-center h-32">
            <div className="text-center">
              <RefreshCw className="h-6 w-6 animate-spin mx-auto mb-2 text-muted-foreground" />
              <p className="text-sm text-muted-foreground">加载中...</p>
            </div>
          </div>
        ) : healthData ? (
          <div className="space-y-4">
            {/* 状态指标 */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">待处理事件</p>
                <p className="text-lg font-semibold flex items-center gap-1">
                  <Clock className="h-4 w-4 text-orange-500" />
                  {healthData.details.pendingEvents}
                </p>
              </div>
              
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">成功率</p>
                <p className="text-lg font-semibold flex items-center gap-1">
                  <TrendingUp className="h-4 w-4 text-green-500" />
                  {healthData.details.successRate.toFixed(1)}%
                </p>
              </div>
              
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">已处理</p>
                <p className="text-lg font-semibold text-green-600">
                  {healthData.details.totalProcessed.toLocaleString()}
                </p>
              </div>
              
              <div className="space-y-1">
                <p className="text-xs text-muted-foreground">失败数</p>
                <p className="text-lg font-semibold text-red-600">
                  {healthData.details.totalFailed.toLocaleString()}
                </p>
              </div>
            </div>

            {/* 最后处理时间 */}
            <div className="pt-3 border-t">
              <p className="text-xs text-muted-foreground mb-1">最后处理时间</p>
              <p className="text-sm">
                {new Date(healthData.details.lastProcessed).toLocaleString('zh-CN')}
              </p>
            </div>

            {/* 状态更新时间 */}
            <div className="text-xs text-muted-foreground text-center">
              更新于: {new Date(healthData.timestamp).toLocaleString('zh-CN')}
            </div>
          </div>
        ) : (
          <div className="flex items-center justify-center h-32">
            <p className="text-sm text-muted-foreground">暂无数据</p>
          </div>
        )}
      </CardContent>
    </Card>
  )
}
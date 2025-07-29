import { ChartAreaInteractive } from "@/components/chart-area-interactive"
import { DataTable } from "@/components/data-table"
import { SectionCards } from "@/components/section-cards"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { useAuth } from "@/lib/auth"
import { 
  Bot, 
  MessageSquare, 
  Settings, 
  Users, 
  Activity, 
  TrendingUp,
  Plus,
  MoreHorizontal
} from "lucide-react"

import data from "@/app/dashboard/data.json"

export default function DashboardPage() {
  const { user } = useAuth()

  const stats = [
    {
      title: "活跃代理",
      value: "12",
      change: "+2.1%",
      trend: "up",
      icon: Bot,
      description: "本月新增2个代理"
    },
    {
      title: "处理任务",
      value: "1,429",
      change: "+15.3%", 
      trend: "up",
      icon: Activity,
      description: "本周完成任务数量"
    },
    {
      title: "对话会话",
      value: "573",
      change: "+8.2%",
      trend: "up", 
      icon: MessageSquare,
      description: "活跃对话会话数"
    },
    {
      title: "系统状态",
      value: "99.9%",
      change: "0%",
      trend: "stable",
      icon: TrendingUp,
      description: "系统可用性"
    }
  ]

  const recentActivities = [
    {
      id: 1,
      type: "agent_created",
      message: "创建了新的AI代理 \"数据分析助手\"",
      time: "2分钟前",
      status: "success"
    },
    {
      id: 2, 
      type: "task_completed",
      message: "完成了批量数据处理任务",
      time: "15分钟前",
      status: "success"
    },
    {
      id: 3,
      type: "conversation_started", 
      message: "新用户开始了对话会话",
      time: "1小时前",
      status: "info"
    },
    {
      id: 4,
      type: "system_update",
      message: "系统配置已更新",
      time: "3小时前", 
      status: "warning"
    }
  ]

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'success': return 'bg-green-100 text-green-800'
      case 'warning': return 'bg-yellow-100 text-yellow-800'
      case 'info': return 'bg-blue-100 text-blue-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getTrendIcon = (trend: string) => {
    if (trend === 'up') return <TrendingUp className="h-4 w-4 text-green-600" />
    return null
  }

  return (
    <div className="flex flex-1 flex-col">
      <div className="@container/main flex flex-1 flex-col gap-2">
        <div className="flex flex-col gap-4 py-4 md:gap-6 md:py-6">
          {/* 欢迎信息 */}
          <div className="px-4 lg:px-6">
            <div className="flex items-center justify-between">
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  欢迎回来，{user?.userName}！
                </h1>
                <p className="text-gray-600 mt-1">
                  这是您的Mark Agent管理控制台
                </p>
              </div>
              <div className="flex gap-2">
                <Button>
                  <Plus className="h-4 w-4 mr-2" />
                  创建代理
                </Button>
                <Button variant="outline">
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </div>
            </div>
          </div>

          {/* 统计卡片 */}
          <div className="px-4 lg:px-6">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
              {stats.map((stat, index) => (
                <Card key={index}>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                      {stat.title}
                    </CardTitle>
                    <stat.icon className="h-4 w-4 text-muted-foreground" />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">{stat.value}</div>
                    <div className="flex items-center text-xs text-muted-foreground">
                      {getTrendIcon(stat.trend)}
                      <span className="ml-1">{stat.change}</span>
                      <span className="ml-1">vs 上月</span>
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      {stat.description}
                    </p>
                  </CardContent>
                </Card>
              ))}
            </div>
          </div>

          {/* 图表区域 */}
          <div className="px-4 lg:px-6">
            <ChartAreaInteractive />
          </div>

          {/* 主要内容区域 */}
          <div className="px-4 lg:px-6">
            <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
              {/* 最近活动 */}
              <div className="lg:col-span-2">
                <Card>
                  <CardHeader>
                    <CardTitle>最近活动</CardTitle>
                    <CardDescription>
                      查看系统中的最新动态
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      {recentActivities.map((activity) => (
                        <div key={activity.id} className="flex items-center gap-4">
                          <div className="flex-shrink-0">
                            <Badge 
                              variant="secondary" 
                              className={getStatusColor(activity.status)}
                            >
                              {activity.type === 'agent_created' && <Bot className="h-3 w-3" />}
                              {activity.type === 'task_completed' && <Activity className="h-3 w-3" />}
                              {activity.type === 'conversation_started' && <MessageSquare className="h-3 w-3" />}
                              {activity.type === 'system_update' && <Settings className="h-3 w-3" />}
                            </Badge>
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-medium text-gray-900">
                              {activity.message}
                            </p>
                            <p className="text-xs text-gray-500">
                              {activity.time}
                            </p>
                          </div>
                        </div>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              </div>

              {/* 快速操作 */}
              <div>
                <Card>
                  <CardHeader>
                    <CardTitle>快速操作</CardTitle>
                    <CardDescription>
                      常用功能入口
                    </CardDescription>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <Button className="w-full justify-start" variant="outline">
                      <Bot className="h-4 w-4 mr-2" />
                      管理代理
                    </Button>
                    <Button className="w-full justify-start" variant="outline">
                      <MessageSquare className="h-4 w-4 mr-2" />
                      对话管理
                    </Button>
                    <Button className="w-full justify-start" variant="outline">
                      <Users className="h-4 w-4 mr-2" />
                      用户管理
                    </Button>
                    <Button className="w-full justify-start" variant="outline">
                      <Settings className="h-4 w-4 mr-2" />
                      系统设置
                    </Button>
                  </CardContent>
                </Card>
              </div>
            </div>
          </div>

          {/* 数据表格 */}
          <div className="px-4 lg:px-6">
            <Card>
              <CardHeader>
                <CardTitle>代理详情</CardTitle>
                <CardDescription>
                  查看和管理您的AI代理
                </CardDescription>
              </CardHeader>
              <CardContent>
                <DataTable data={data} />
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  )
} 
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { useAuth } from '@/lib/auth'
import { ChevronRight, Bot, MessageSquare, Settings, Zap, BarChart3 } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { ModeToggle } from '@/components/mode-toggle'
import { StatisticsDashboard } from '@/components/statistics/statistics-dashboard'

export function HomePage() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()

  const features = [
    {
      icon: Bot,
      title: 'AI代理管理',
      description: '集中管理和配置您的AI代理，实现智能化工作流程。',
    },
    {
      icon: MessageSquare,
      title: 'MCP协议支持',
      description: '完全兼容Model Context Protocol，让AI代理之间无缝协作。',
    },
    {
      icon: Settings,
      title: '灵活配置',
      description: '自定义代理行为、权限和工作环境，满足不同业务需求。',
    },
    {
      icon: Zap,
      title: '高效执行',
      description: '优化的任务调度和资源管理，确保代理高效运行。',
    },
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
      {/* 头部导航 */}
      <header className="border-b bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <div className="h-8 w-8 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center">
              <Bot className="h-5 w-5 text-white" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Mark Agent</h1>
          </div>
          <div className="flex items-center space-x-4">
            <ModeToggle />
            {isAuthenticated ? (
              <Button onClick={() => navigate('/admin/dashboard')}>
                进入控制台
                <ChevronRight className="ml-2 h-4 w-4" />
              </Button>
            ) : (
              <Button onClick={() => navigate('/login')}>
                登录
                <ChevronRight className="ml-2 h-4 w-4" />
              </Button>
            )}
          </div>
        </div>
      </header>

      {/* 主要内容 */}
      <main className="container mx-auto px-4 py-16">
        {/* 英雄区域 */}
        <div className="text-center mb-12">
          <Badge variant="secondary" className="mb-4">
            基于Model Context Protocol
          </Badge>
          <h2 className="text-5xl font-bold text-gray-900 dark:text-white mb-6">
            智能代理管理平台
          </h2>
          <p className="text-xl text-gray-600 dark:text-gray-300 mb-8 max-w-3xl mx-auto">
            Mark Agent是一个现代化的AI代理管理平台，支持MCP协议，让您轻松管理、配置和协调多个AI代理，
            实现复杂的智能化工作流程。
          </p>
          {!isAuthenticated && (
            <div className="flex justify-center space-x-4">
              <Button size="lg" onClick={() => navigate('/docs/mcp-integration')}>
                开始使用
                <ChevronRight className="ml-2 h-5 w-5" />
              </Button>
              <Button variant="outline" size="lg" onClick={() => navigate('/docs/mcp-integration')}>
                了解更多
              </Button>
            </div>
          )}
        </div>

        {/* 内容标签页 */}
        <Tabs defaultValue="overview" className="w-full">
          <TabsList className="grid w-full grid-cols-2 mb-8">
            <TabsTrigger value="overview" className="flex items-center gap-2">
              <Bot className="h-4 w-4" />
              平台介绍
            </TabsTrigger>
            <TabsTrigger value="statistics" className="flex items-center gap-2">
              <BarChart3 className="h-4 w-4" />
              使用统计
            </TabsTrigger>
          </TabsList>

          {/* 平台介绍标签页 */}
          <TabsContent value="overview" className="space-y-16">
            {/* 功能特性 */}
            <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
          {features.map((feature, index) => (
            <Card key={index} className="text-center hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="mx-auto h-12 w-12 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center mb-4">
                  <feature.icon className="h-6 w-6 text-white" />
                </div>
                <CardTitle className="text-lg">{feature.title}</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription className="text-sm">
                  {feature.description}
                </CardDescription>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* MCP协议介绍 */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow-xl p-8 mb-16">
          <div className="text-center mb-8">
            <h3 className="text-3xl font-bold text-gray-900 dark:text-white mb-4">
              什么是Model Context Protocol？
            </h3>
            <p className="text-lg text-gray-600 dark:text-gray-300 max-w-4xl mx-auto">
              Model Context Protocol (MCP) 是一个开放标准，用于AI应用程序和外部数据源及工具之间的安全连接。
              它提供了一个通用的协议，让AI模型能够安全地访问和操作各种外部资源。
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="h-16 w-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Settings className="h-8 w-8 text-green-600" />
              </div>
              <h4 className="text-xl font-semibold mb-2 dark:text-white">标准化接口</h4>
              <p className="text-gray-600 dark:text-gray-300">
                提供统一的API接口，简化AI应用与外部系统的集成。
              </p>
            </div>
            <div className="text-center">
              <div className="h-16 w-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <MessageSquare className="h-8 w-8 text-blue-600" />
              </div>
              <h4 className="text-xl font-semibold mb-2 dark:text-white">安全通信</h4>
              <p className="text-gray-600 dark:text-gray-300">
                内置安全机制，确保AI代理与外部资源的安全交互。
              </p>
            </div>
            <div className="text-center">
              <div className="h-16 w-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Zap className="h-8 w-8 text-purple-600" />
              </div>
              <h4 className="text-xl font-semibold mb-2 dark:text-white">高效协作</h4>
              <p className="text-gray-600 dark:text-gray-300">
                支持多个AI代理之间的协作，实现复杂任务的分工处理。
              </p>
            </div>
          </div>
        </div>

        {/* 使用场景 */}
        <div className="text-center">
          <h3 className="text-3xl font-bold text-gray-900 dark:text-white mb-8">
            适用场景
          </h3>
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            <Card className="text-left">
              <CardHeader>
                <CardTitle>企业自动化</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600 dark:text-gray-300">
                  自动化处理企业内部流程，如数据分析、报告生成、客户服务等。
                </p>
              </CardContent>
            </Card>
            <Card className="text-left">
              <CardHeader>
                <CardTitle>开发工具集成</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600 dark:text-gray-300">
                  与开发工具链集成，自动化代码审查、测试、部署等开发流程。
                </p>
              </CardContent>
            </Card>
            <Card className="text-left">
              <CardHeader>
                <CardTitle>智能助手</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="text-gray-600 dark:text-gray-300">
                  构建能够访问多种数据源和工具的智能助手，提供全面的服务支持。
                </p>
              </CardContent>
            </Card>
          </div>
            </div>
          </TabsContent>

          {/* 使用统计标签页 */}
          <TabsContent value="statistics">
            <StatisticsDashboard />
          </TabsContent>
        </Tabs>
      </main>

      {/* 页脚 */}
      <footer className="bg-gray-900 text-white py-8">
        <div className="container mx-auto px-4 text-center">
          <p className="text-gray-400">
            © 2025 Mark Agent. 基于Model Context Protocol构建的智能代理管理平台。
          </p>
        </div>
      </footer>
    </div>
  )
} 
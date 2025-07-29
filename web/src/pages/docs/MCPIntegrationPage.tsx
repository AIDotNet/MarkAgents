import { useState, useEffect } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { 
  Copy, 
  ExternalLink, 
  FileText, 
  Code2, 
  Zap,
  CheckCircle,
  Download,
  Lightbulb,
  Settings
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'

export function MCPIntegrationPage() {
  const navigate = useNavigate()
  const [copiedStates, setCopiedStates] = useState<Record<string, boolean>>({})

  const copyToClipboard = async (text: string, id: string) => {
    try {
      await navigator.clipboard.writeText(text)
      setCopiedStates(prev => ({ ...prev, [id]: true }))
      setTimeout(() => {
        setCopiedStates(prev => ({ ...prev, [id]: false }))
      }, 2000)
    } catch (err) {
      console.error('Failed to copy:', err)
    }
  }

  const cursorConfig = `{
  "mcpServers": {
    "todo": {
      "url": "https://agent.mark-chat.chat/mcp"
    }
  }
}`

  const copilotConfig = `{
  "servers": {
    "todo": {
      "url": "https://agent.mark-chat.chat/mcp",
      "type": "http"
    }
  }
}`

  const claudeDesktopConfig = `{
  "mcpServers": {
    "markagent": {
      "command": "node",
      "args": ["/path/to/markagent-mcp-server/dist/index.js"],
      "env": {
        "MARKAGENT_SERVER_URL": "https://agent.mark-chat.chat/mcp"
      }
    }
  }
}`

  const features = [
    {
      icon: Zap,
      title: "TodoWrite 工具",
      description: "智能任务管理和待办事项工具，支持优先级和状态跟踪",
    },
    {
      icon: CheckCircle,
      title: "实时统计",
      description: "详细的使用统计和性能监控，帮助优化工作流程",
    },
    {
      icon: Settings,
      title: "灵活配置",
      description: "支持多种IDE和AI客户端，轻松集成到现有工作环境",
    }
  ]

  const steps = [
    {
      step: 1,
      title: "选择你的IDE",
      description: "根据使用的开发环境选择对应的配置方式",
    },
    {
      step: 2,
      title: "复制配置代码",
      description: "复制下方提供的JSON配置到相应的配置文件中",
    },
    {
      step: 3,
      title: "重启IDE",
      description: "重启IDE让配置生效，开始使用MarkAgent工具",
    }
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 dark:from-gray-900 dark:to-gray-800">
      {/* 头部导航 */}
      <header className="border-b bg-white/80 dark:bg-gray-900/80 backdrop-blur-sm">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <Button 
            variant="ghost" 
            onClick={() => navigate('/')}
            className="flex items-center gap-2"
          >
            ← 返回首页
          </Button>
          <Badge variant="secondary">
            MCP 集成文档
          </Badge>
        </div>
      </header>

      <main className="container mx-auto px-4 py-16 max-w-4xl">
        {/* 文档标题 */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
            MCP 集成指南
          </h1>
          <p className="text-xl text-gray-600 dark:text-gray-300 mb-6">
            将 MarkAgent 集成到你的 AI 开发环境中，享受智能工具带来的效率提升
          </p>
          <div className="flex justify-center gap-4">
            <Badge variant="outline" className="px-3 py-1">
              <Code2 className="h-3 w-3 mr-1" />
              Model Context Protocol
            </Badge>
            <Badge variant="outline" className="px-3 py-1">
              <Zap className="h-3 w-3 mr-1" />
              即插即用
            </Badge>
          </div>
        </div>

        {/* 功能特性 */}
        <Card className="mb-12">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Lightbulb className="h-5 w-5" />
              功能特性
            </CardTitle>
            <CardDescription>
              MarkAgent 为你的 AI 工作流程提供强大的工具支持
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid md:grid-cols-3 gap-6">
              {features.map((feature, index) => (
                <div key={index} className="text-center">
                  <div className="h-12 w-12 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center mx-auto mb-4">
                    <feature.icon className="h-6 w-6 text-white" />
                  </div>
                  <h3 className="font-semibold mb-2">{feature.title}</h3>
                  <p className="text-sm text-muted-foreground">{feature.description}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* 快速开始 */}
        <Card className="mb-12">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Download className="h-5 w-5" />
              快速开始
            </CardTitle>
            <CardDescription>
              三步完成 MCP 服务器集成
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid md:grid-cols-3 gap-6">
              {steps.map((step, index) => (
                <div key={index} className="flex flex-col items-center text-center">
                  <div className="h-10 w-10 bg-primary rounded-full flex items-center justify-center text-white font-bold mb-4">
                    {step.step}
                  </div>
                  <h3 className="font-semibold mb-2">{step.title}</h3>
                  <p className="text-sm text-muted-foreground">{step.description}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* 配置代码 */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              配置代码
            </CardTitle>
            <CardDescription>
              根据你使用的 IDE 选择对应的配置
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Tabs defaultValue="cursor" className="w-full">
              <TabsList className="grid w-full grid-cols-3">
                <TabsTrigger value="cursor">Cursor</TabsTrigger>
                <TabsTrigger value="copilot">GitHub Copilot</TabsTrigger>
                <TabsTrigger value="claude">Claude Desktop</TabsTrigger>
              </TabsList>

              <TabsContent value="cursor" className="space-y-4">
                <div className="space-y-2">
                  <h3 className="text-lg font-semibold">Cursor IDE 配置</h3>
                  <p className="text-sm text-muted-foreground">
                    将以下配置添加到 Cursor 的 MCP 设置中：
                  </p>
                </div>
                <div className="relative">
                  <pre className="bg-muted p-4 rounded-lg text-sm overflow-x-auto">
                    <code>{cursorConfig}</code>
                  </pre>
                  <Button
                    size="sm"
                    variant="outline"
                    className="absolute top-2 right-2"
                    onClick={() => copyToClipboard(cursorConfig, 'cursor')}
                  >
                    {copiedStates.cursor ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <div className="bg-blue-50 dark:bg-blue-950/20 p-4 rounded-lg">
                  <p className="text-sm text-blue-700 dark:text-blue-400">
                    <strong>配置路径：</strong> Cursor → 设置 → MCP Servers → 添加新服务器
                  </p>
                </div>
              </TabsContent>

              <TabsContent value="copilot" className="space-y-4">
                <div className="space-y-2">
                  <h3 className="text-lg font-semibold">GitHub Copilot 配置</h3>
                  <p className="text-sm text-muted-foreground">
                    在 VS Code 中为 GitHub Copilot 配置 MCP 服务器：
                  </p>
                </div>
                <div className="relative">
                  <pre className="bg-muted p-4 rounded-lg text-sm overflow-x-auto">
                    <code>{copilotConfig}</code>
                  </pre>
                  <Button
                    size="sm"
                    variant="outline"
                    className="absolute top-2 right-2"
                    onClick={() => copyToClipboard(copilotConfig, 'copilot')}
                  >
                    {copiedStates.copilot ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <div className="bg-green-50 dark:bg-green-950/20 p-4 rounded-lg">
                  <p className="text-sm text-green-700 dark:text-green-400">
                    <strong>配置文件：</strong> ~/.vscode/mcp-servers.json 或工作区的 .vscode/settings.json
                  </p>
                </div>
              </TabsContent>

              <TabsContent value="claude" className="space-y-4">
                <div className="space-y-2">
                  <h3 className="text-lg font-semibold">Claude Desktop 配置</h3>
                  <p className="text-sm text-muted-foreground">
                    为 Claude Desktop 应用配置 MCP 服务器：
                  </p>
                </div>
                <div className="relative">
                  <pre className="bg-muted p-4 rounded-lg text-sm overflow-x-auto">
                    <code>{claudeDesktopConfig}</code>
                  </pre>
                  <Button
                    size="sm"
                    variant="outline"
                    className="absolute top-2 right-2"
                    onClick={() => copyToClipboard(claudeDesktopConfig, 'claude')}
                  >
                    {copiedStates.claude ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <Copy className="h-4 w-4" />
                    )}
                  </Button>
                </div>
                <div className="bg-purple-50 dark:bg-purple-950/20 p-4 rounded-lg">
                  <p className="text-sm text-purple-700 dark:text-purple-400">
                    <strong>配置文件：</strong> ~/Library/Application Support/Claude/claude_desktop_config.json (macOS)
                  </p>
                </div>
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>

        {/* 底部链接 */}
        <div className="mt-12 text-center">
          <div className="flex justify-center gap-4">
            <Button 
              variant="outline" 
              onClick={() => navigate('/admin/dashboard')}
              className="flex items-center gap-2"
            >
              <ExternalLink className="h-4 w-4" />
              查看使用统计
            </Button>
            <Button 
              onClick={() => navigate('/')}
              className="flex items-center gap-2"
            >
              开始使用 MarkAgent
            </Button>
          </div>
          <p className="text-sm text-muted-foreground mt-4">
            有问题？查看我们的 <a href="#" className="text-primary hover:underline">常见问题</a> 或 <a href="#" className="text-primary hover:underline">联系支持</a>
          </p>
        </div>
      </main>
    </div>
  )
}
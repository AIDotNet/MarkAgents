import { useState, } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Separator } from '@/components/ui/separator'
import {
  Copy,
  ExternalLink,
  FileText,
  CheckCircle,
  Download,
  Code,
  Settings,
  BookOpen,
  Sparkles,
  ArrowRight,
  Zap,
} from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { vscDarkPlus, oneLight } from 'react-syntax-highlighter/dist/esm/styles/prism'

export function MCPIntegrationPage() {
  const navigate = useNavigate()
  const [copiedStates, setCopiedStates] = useState<Record<string, boolean>>({})
  const [isDarkMode, setIsDarkMode] = useState(
    window.matchMedia('(prefers-color-scheme: dark)').matches
  )

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
      "url": "${window.location.origin}/mcp"
    }
  }
}`

  const copilotConfig = `{
  "servers": {
    "todo": {
      "url": "${window.location.origin}/mcp",
      "type": "http"
    }
  }
}`

  const steps = [
    {
      step: 1,
      title: "选择你的IDE",
      description: "根据使用的开发环境选择对应的配置方式",
      icon: <Settings className="h-6 w-6" />,
      color: "from-blue-500 to-blue-600"
    },
    {
      step: 2,
      title: "复制配置代码",
      description: "复制下方提供的JSON配置到相应的配置文件中",
      icon: <Code className="h-6 w-6" />,
      color: "from-purple-500 to-purple-600"
    },
    {
      step: 3,
      title: "重启IDE",
      description: "重启IDE让配置生效，开始使用MarkAgent工具",
      icon: <Zap className="h-6 w-6" />,
      color: "from-green-500 to-green-600"
    }
  ]

  const features = [
    {
      title: "智能Todo管理",
      description: "AI驱动的任务创建、更新和追踪",
      icon: <CheckCircle className="h-5 w-5" />,
    },
    {
      title: "实时同步",
      description: "跨设备实时同步你的任务列表",
      icon: <ArrowRight className="h-5 w-5" />,
    },
    {
      title: "智能提醒",
      description: "基于上下文的智能任务提醒",
      icon: <Sparkles className="h-5 w-5" />,
    },
  ]

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 dark:from-gray-900 dark:via-gray-900 dark:to-gray-800">
      {/* 头部导航 */}
      <header className="border-b bg-white/90 dark:bg-gray-900/90 backdrop-blur-md sticky top-0 z-50">
        <div className="container mx-auto px-4 py-4 flex items-center justify-between">
          <Button
            variant="ghost"
            onClick={() => navigate('/')}
            className="flex items-center gap-2 hover:bg-blue-50 dark:hover:bg-gray-800"
          >
            ← 返回首页
          </Button>
          <Badge variant="secondary" className="bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
            <BookOpen className="h-4 w-4 mr-1" />
            MCP 集成文档
          </Badge>
        </div>
      </header>

      <main className="container mx-auto px-4 py-12 max-w-6xl">
        {/* 英雄区域 */}
        <div className="text-center mb-16">
          <div className="inline-flex items-center gap-2 bg-blue-100 dark:bg-blue-900/30 text-blue-800 dark:text-blue-200 px-4 py-2 rounded-full text-sm font-medium mb-6">
            <Sparkles className="h-4 w-4" />
            轻松集成 MarkAgent
          </div>
        </div>

        {/* 配置代码 */}
        <Card className="border-0 shadow-xl bg-white/80 dark:bg-gray-800/80 backdrop-blur-sm">
          <CardContent>
            <Tabs defaultValue="cursor" className="w-full">
              <TabsList className="grid w-full grid-cols-2 bg-gray-100 dark:bg-gray-800 p-1 rounded-lg">
                <TabsTrigger value="cursor" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 data-[state=active]:shadow-sm">
                  Cursor
                </TabsTrigger>
                <TabsTrigger value="copilot" className="data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 data-[state=active]:shadow-sm">
                  GitHub Copilot
                </TabsTrigger>
              </TabsList>

              <TabsContent value="cursor" className="space-y-6 mt-6">
                <div className="space-y-3">
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white">Cursor IDE 配置</h3>
                  <p className="text-gray-600 dark:text-gray-300">
                    将以下配置添加到 Cursor 的 MCP 设置中，即可开始使用 MarkAgent 功能：
                  </p>
                </div>
                
                <div className="relative group">
                  <div className="absolute inset-0 bg-gradient-to-r from-blue-500/20 to-purple-500/20 rounded-lg blur opacity-0 transition duration-300"></div>
                  <div className="relative bg-gray-50 dark:bg-gray-900 rounded-lg overflow-hidden border">
                    <div className="flex items-center justify-between bg-gray-100 dark:bg-gray-800 px-4 py-2 border-b">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">cursor-mcp-config.json</span>
                      <Button
                        size="sm"
                        variant="ghost"
                        className="h-8 px-3 hover:bg-gray-200"
                        onClick={() => copyToClipboard(cursorConfig, 'cursor')}
                      >
                        {copiedStates.cursor ? (
                          <>
                            <CheckCircle className="h-4 w-4 text-green-600 mr-1" />
                            已复制
                          </>
                        ) : (
                          <>
                            <Copy className="h-4 w-4 mr-1" />
                            复制
                          </>
                        )}
                      </Button>
                    </div>
                    <SyntaxHighlighter
                      language="json"
                      style={isDarkMode ? vscDarkPlus : oneLight}
                      customStyle={{
                        margin: 0,
                        padding: '1rem',
                        background: 'transparent',
                        fontSize: '14px',
                      }}
                      wrapLongLines={true}
                    >
                      {cursorConfig}
                    </SyntaxHighlighter>
                  </div>
                </div>
                
                <div className="bg-gradient-to-r from-blue-50 to-blue-100 dark:from-blue-950/30 dark:to-blue-900/30 p-6 rounded-lg border border-blue-200 dark:border-blue-800">
                  <div className="flex items-start gap-3">
                    <div className="p-1 bg-blue-500 rounded">
                      <Settings className="h-4 w-4 text-white" />
                    </div>
                    <div>
                      <p className="font-medium text-blue-900 dark:text-blue-100 mb-1">配置路径</p>
                      <p className="text-blue-700 dark:text-blue-300 text-sm">
                        Cursor → 设置 → Features → MCP Servers → 添加新服务器
                      </p>
                    </div>
                  </div>
                </div>
              </TabsContent>

              <TabsContent value="copilot" className="space-y-6 mt-6">
                <div className="space-y-3">
                  <h3 className="text-xl font-semibold text-gray-900 dark:text-white">GitHub Copilot 配置</h3>
                  <p className="text-gray-600 dark:text-gray-300">
                    在 VS Code 中为 GitHub Copilot 配置 MCP 服务器：
                  </p>
                </div>
                
                <div className="relative group">
                  <div className="absolute inset-0 bg-gradient-to-r from-purple-500/20 to-pink-500/20 rounded-lg blur opacity-0 transition duration-300"></div>
                  <div className="relative bg-gray-50 dark:bg-gray-900 rounded-lg overflow-hidden border">
                    <div className="flex items-center justify-between bg-gray-100 dark:bg-gray-800 px-4 py-2 border-b">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">mcp-servers.json</span>
                      <Button
                        size="sm"
                        variant="ghost"
                        className="h-8 px-3"
                        onClick={() => copyToClipboard(copilotConfig, 'copilot')}
                      >
                        {copiedStates.copilot ? (
                          <>
                            <CheckCircle className="h-4 w-4 text-green-600 mr-1" />
                            已复制
                          </>
                        ) : (
                          <>
                            <Copy className="h-4 w-4 mr-1" />
                            复制
                          </>
                        )}
                      </Button>
                    </div>
                    <SyntaxHighlighter
                      language="json"
                      style={isDarkMode ? vscDarkPlus : oneLight}
                      customStyle={{
                        margin: 0,
                        padding: '1rem',
                        background: 'transparent',
                        fontSize: '14px',
                      }}
                      wrapLongLines={true}
                    >
                      {copilotConfig}
                    </SyntaxHighlighter>
                  </div>
                </div>
                
                <div className="bg-gradient-to-r from-green-50 to-emerald-100 dark:from-green-950/30 dark:to-emerald-900/30 p-6 rounded-lg border border-green-200 dark:border-green-800">
                  <div className="flex items-start gap-3">
                    <div className="p-1 bg-green-500 rounded">
                      <FileText className="h-4 w-4 text-white" />
                    </div>
                    <div>
                      <p className="font-medium text-green-900 dark:text-green-100 mb-1">配置文件位置</p>
                      <p className="text-green-700 dark:text-green-300 text-sm">
                        <code className="bg-green-100 dark:bg-green-900/50 px-2 py-1 rounded">~/.vscode/mcp-servers.json</code> 或工作区的 <code className="bg-green-100 dark:bg-green-900/50 px-2 py-1 rounded">.vscode/settings.json</code>
                      </p>
                    </div>
                  </div>
                </div>
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>
        <Separator className="my-12" />
      </main>
    </div>
  )
}
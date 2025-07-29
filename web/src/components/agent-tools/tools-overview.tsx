import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { 
  Bot, 
  Code, 
  Settings, 
  Layers,
  Clock,
  CheckCircle,
  ArrowRight
} from "lucide-react";
import type { ToolOverviewInfo } from "@/lib/agent-tools-api";

interface ToolsOverviewProps {
  overview: ToolOverviewInfo | null;
  loading?: boolean;
}

export function ToolsOverview({ overview, loading = false }: ToolsOverviewProps) {
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
    );
  }

  if (!overview) {
    return (
      <Card className="border-gray-200 dark:border-gray-700">
        <CardContent className="flex flex-col items-center justify-center h-[200px]">
          <Bot className="h-12 w-12 text-gray-400 mb-4" />
          <p className="text-gray-500 dark:text-gray-400">暂无工具概览数据</p>
        </CardContent>
      </Card>
    );
  }

  const stats = [
    {
      title: "总工具数",
      value: overview.totalTools.toString(),
      description: "可用的MCP工具总数",
      icon: Bot,
      color: "text-blue-600 dark:text-blue-400"
    },
    {
      title: "工具类别",
      value: overview.categories.toString(),
      description: "按功能分类的类别数",
      icon: Layers,
      color: "text-green-600 dark:text-green-400"
    },
    {
      title: "异步工具",
      value: overview.asyncTools.toString(),
      description: "支持异步执行的工具",
      icon: Clock,
      color: "text-orange-600 dark:text-orange-400"
    },
    {
      title: "总参数数",
      value: overview.parameterStats.totalParameters.toString(),
      description: "所有工具的参数总和",
      icon: Settings,
      color: "text-purple-600 dark:text-purple-400"
    }
  ];

  return (
    <div className="space-y-6">
      {/* 概览卡片 */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, index) => (
          <Card key={index} className="border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                {stat.title}
              </CardTitle>
              <stat.icon className={`h-4 w-4 ${stat.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400 mt-1">
                {stat.description}
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* 详细统计 */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* 参数统计 */}
        <Card className="border-gray-200 dark:border-gray-700">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Code className="h-5 w-5" />
              参数统计
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex justify-between items-center">
              <span className="text-sm font-medium">必需参数</span>
              <Badge variant="default" className="bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                {overview.parameterStats.requiredParameters}
              </Badge>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm font-medium">可选参数</span>
              <Badge variant="secondary" className="bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200">
                {overview.parameterStats.optionalParameters}
              </Badge>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm font-medium">复杂类型</span>
              <Badge variant="outline" className="border-purple-200 text-purple-800 dark:border-purple-700 dark:text-purple-200">
                {overview.parameterStats.complexTypeParameters}
              </Badge>
            </div>
          </CardContent>
        </Card>

        {/* 类别分布 */}
        <Card className="border-gray-200 dark:border-gray-700">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Layers className="h-5 w-5" />
              类别分布
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {Object.entries(overview.toolsByCategory)
              .sort(([,a], [,b]) => b - a)
              .map(([category, count]) => (
                <div key={category} className="flex justify-between items-center">
                  <span className="text-sm font-medium flex items-center gap-2">
                    <div className="w-2 h-2 bg-blue-500 rounded-full" />
                    {category}
                  </span>
                  <div className="flex items-center gap-2">
                    <span className="text-sm text-gray-600 dark:text-gray-400">{count}</span>
                    <ArrowRight className="h-3 w-3 text-gray-400" />
                  </div>
                </div>
              ))}
          </CardContent>
        </Card>
      </div>

      {/* 执行类型分布 */}
      <Card className="border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CheckCircle className="h-5 w-5" />
            执行类型分布
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-center space-x-8">
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600 dark:text-blue-400">
                {overview.asyncTools}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">异步工具</div>
              <div className="text-xs text-gray-500 mt-1">
                {((overview.asyncTools / overview.totalTools) * 100).toFixed(1)}%
              </div>
            </div>
            <div className="h-12 w-px bg-gray-300 dark:bg-gray-600" />
            <div className="text-center">
              <div className="text-3xl font-bold text-green-600 dark:text-green-400">
                {overview.syncTools}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400">同步工具</div>
              <div className="text-xs text-gray-500 mt-1">
                {((overview.syncTools / overview.totalTools) * 100).toFixed(1)}%
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
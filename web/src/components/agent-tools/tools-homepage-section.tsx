"use client"

import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { 
  Search, 
  Bot, 
  Layers,
  Hash,
  Clock,
  AlertCircle,
  RefreshCw,
  ChevronRight,
  Filter,
  BookOpen,
  ExternalLink
} from "lucide-react";
import { useAgentTools } from "@/lib/agent-tools-api";
import { ToolCompactCard } from "./tool-compact-card";

interface ToolsHomepageSectionProps {
  onViewAll?: () => void;
  onViewDocs?: () => void;
}

export function ToolsHomepageSection({ onViewAll, onViewDocs }: ToolsHomepageSectionProps) {
  const {
    tools,
    overview,
    loading,
    error,
    fetchAllTools,
    fetchToolsOverview,
    fetchToolsByCategory,
    searchTools,
  } = useAgentTools();

  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    if (!isInitialized) {
      const initializeData = async () => {
        try {
          await Promise.all([
            fetchAllTools(),
            fetchToolsOverview(),
            fetchToolsByCategory()
          ]);
          setIsInitialized(true);
        } catch (err) {
          console.error('Failed to initialize tools data:', err);
        }
      };
      
      initializeData();
    }
  }, [isInitialized, fetchAllTools, fetchToolsOverview, fetchToolsByCategory]);

  const handleSearch = async (keyword: string) => {
    if (!keyword.trim()) {
      await fetchAllTools();
      return;
    }
    await searchTools(keyword);
  };

  const filteredTools = selectedCategory === 'all' 
    ? tools 
    : tools.filter(tool => tool.category === selectedCategory);

  const displayTools = filteredTools.slice(0, 6); // 首页只显示前6个工具
  const categories = overview ? Object.keys(overview.toolsByCategory) : [];

  if (error && !tools.length && !overview) {
    return (
      <Card className="border-gray-200 dark:border-gray-700">
        <CardContent className="flex flex-col items-center justify-center h-[300px]">
          <AlertCircle className="h-12 w-12 text-red-500 mb-4" />
          <h3 className="text-lg font-semibold mb-2">工具信息加载失败</h3>
          <p className="text-gray-600 dark:text-gray-400 mb-4 text-center">
            {error}
          </p>
          <Button onClick={() => window.location.reload()} variant="outline">
            <RefreshCw className="h-4 w-4 mr-2" />
            重新加载
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* MCP 接入提示 */}
      <Card className="border-blue-200 dark:border-blue-800 bg-blue-50 dark:bg-blue-950/20">
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div className="flex items-start gap-3">
              <div className="p-2 bg-blue-100 dark:bg-blue-900/30 rounded-lg">
                <BookOpen className="h-5 w-5 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <h3 className="font-semibold text-blue-900 dark:text-blue-100 mb-1">
                  将工具集成到您的 AI 应用
                </h3>
                <p className="text-sm text-blue-800 dark:text-blue-200">
                  通过 Model Context Protocol (MCP) 轻松接入这些强大的 AI 工具，
                  支持 Claude Desktop、Cursor、Windsurf 等主流 AI 客户端。
                </p>
              </div>
            </div>
            <Button 
              onClick={onViewDocs}
              className="bg-blue-600 hover:bg-blue-700 text-white shrink-0"
            >
              <BookOpen className="mr-2 h-4 w-4" />
              查看接入文档
              <ExternalLink className="ml-2 h-3 w-3" />
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* 工具概览统计 */}
      {overview && (
        <div className="grid gap-4 md:grid-cols-4">
          <Card className="border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">可用工具</CardTitle>
              <Bot className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{overview.totalTools}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">
                MCP 工具总数
              </p>
            </CardContent>
          </Card>

          <Card className="border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">工具类别</CardTitle>
              <Layers className="h-4 w-4 text-green-600 dark:text-green-400" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{overview.categories}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">
                功能分类数量
              </p>
            </CardContent>
          </Card>

          <Card className="border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">总参数数</CardTitle>
              <Hash className="h-4 w-4 text-purple-600 dark:text-purple-400" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{overview.parameterStats.totalParameters}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">
                所有工具参数总和
              </p>
            </CardContent>
          </Card>

          <Card className="border-gray-200 dark:border-gray-700">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">异步工具</CardTitle>
              <Clock className="h-4 w-4 text-orange-600 dark:text-orange-400" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{overview.asyncTools}</div>
              <p className="text-xs text-gray-600 dark:text-gray-400">
                支持异步执行
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* 搜索和筛选 */}
      <Card className="border-gray-200 dark:border-gray-700">
        <CardContent className="pt-6">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  placeholder="搜索工具名称、描述或参数..."
                  value={searchKeyword}
                  onChange={(e) => setSearchKeyword(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      handleSearch(searchKeyword);
                    }
                  }}
                  className="pl-10"
                />
              </div>
            </div>
            <div className="flex items-center gap-2">
              <Filter className="h-4 w-4 text-gray-500" />
              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                className="px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-md bg-white dark:bg-gray-800 text-sm min-w-[120px]"
              >
                <option value="all">所有类别</option>
                {categories.map(category => (
                  <option key={category} value={category}>{category}</option>
                ))}
              </select>
            </div>
            <Button onClick={() => handleSearch(searchKeyword)} size="sm">
              搜索
            </Button>
          </div>
          
          {searchKeyword && (
            <div className="mt-3 flex items-center gap-2">
              <Badge variant="secondary">
                搜索: "{searchKeyword}"
              </Badge>
              <Button 
                variant="ghost" 
                size="sm" 
                onClick={() => {
                  setSearchKeyword('');
                  fetchAllTools();
                }}
              >
                清除
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* 工具展示区域 */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="text-xl font-semibold">
            可用工具 
            {filteredTools.length > 0 && (
              <span className="text-sm font-normal text-gray-600 dark:text-gray-400 ml-2">
                ({filteredTools.length} 个工具)
              </span>
            )}
          </h3>
          {filteredTools.length > 6 && (
            <Button variant="outline" onClick={onViewAll}>
              查看全部
              <ChevronRight className="ml-2 h-4 w-4" />
            </Button>
          )}
        </div>

        {loading ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
            {Array.from({ length: 6 }).map((_, i) => (
              <Card key={i} className="animate-pulse border-gray-200 dark:border-gray-700">
                <CardHeader>
                  <div className="h-5 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                </CardHeader>
                <CardContent>
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-full mb-2"></div>
                  <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
                </CardContent>
              </Card>
            ))}
          </div>
        ) : displayTools.length > 0 ? (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2">
            {displayTools.map((tool, index) => (
              <ToolCompactCard 
                key={tool.name} 
                tool={tool}
                showDetails={index < 2} // 前两个工具默认展开
              />
            ))}
          </div>
        ) : (
          <Card className="border-gray-200 dark:border-gray-700">
            <CardContent className="flex flex-col items-center justify-center h-[200px]">
              <Bot className="h-12 w-12 text-gray-400 mb-4" />
              <p className="text-gray-500 dark:text-gray-400">
                {searchKeyword ? '没有找到匹配的工具' : '暂无工具数据'}
              </p>
            </CardContent>
          </Card>
        )}

        {/* 底部操作区域 */}
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-4">
          {filteredTools.length > 6 && (
            <Button variant="outline" onClick={onViewAll}>
              查看更多工具 ({filteredTools.length - 6} 个)
              <ChevronRight className="ml-2 h-4 w-4" />
            </Button>
          )}
          <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
            <span>想要在您的 AI 应用中使用这些工具？</span>
            <Button variant="link" onClick={onViewDocs} className="p-0 h-auto text-blue-600 dark:text-blue-400">
              查看 MCP 接入指南
              <ExternalLink className="ml-1 h-3 w-3" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
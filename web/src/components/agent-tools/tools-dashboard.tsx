"use client"

import { useEffect, useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { 
  RefreshCw, 
  Search, 
  Bot, 
  AlertCircle,
  BarChart3,
  Layers,
  Filter
} from "lucide-react";
import { useAgentTools } from "@/lib/agent-tools-api";
import { ToolsOverview } from "./tools-overview";
import { ToolDetailCard } from "./tool-detail-card";

export function ToolsDashboard() {
  const {
    tools,
    overview,
    categorizedTools,
    loading,
    error,
    fetchAllTools,
    fetchToolsOverview,
    fetchToolsByCategory,
    searchTools,
  } = useAgentTools();

  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [isSearching, setIsSearching] = useState(false);

  useEffect(() => {
    // 初始化时加载数据
    const initializeData = async () => {
      await Promise.all([
        fetchAllTools(),
        fetchToolsOverview(),
        fetchToolsByCategory()
      ]);
    };
    
    initializeData();
  }, []);

  const handleSearch = async (keyword: string) => {
    if (!keyword.trim()) {
      await fetchAllTools();
      setIsSearching(false);
      return;
    }
    
    setIsSearching(true);
    await searchTools(keyword);
  };

  const handleRefresh = async () => {
    if (isSearching) {
      await searchTools(searchKeyword);
    } else {
      await Promise.all([
        fetchAllTools(),
        fetchToolsOverview(),
        fetchToolsByCategory()
      ]);
    }
  };

  const filteredTools = selectedCategory === 'all' 
    ? tools 
    : tools.filter(tool => tool.category === selectedCategory);

  const categories = overview ? Object.keys(overview.toolsByCategory) : [];

  // 错误状态
  if (error && !tools.length && !overview) {
    return (
      <Card className="border-gray-200 dark:border-gray-700">
        <CardContent className="flex flex-col items-center justify-center h-[400px]">
          <AlertCircle className="h-12 w-12 text-red-500 mb-4" />
          <h3 className="text-lg font-semibold mb-2">数据加载失败</h3>
          <p className="text-gray-600 dark:text-gray-400 mb-4 text-center">
            {error}
          </p>
          <Button onClick={handleRefresh} variant="outline">
            <RefreshCw className="h-4 w-4 mr-2" />
            重试
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-6">
      {/* 页面标题和操作 */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl font-bold">AgentTools 工具管理</h2>
          <p className="text-gray-600 dark:text-gray-400">
            查看和管理所有可用的 MCP 工具及其详细信息
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button 
            onClick={handleRefresh} 
            variant="outline" 
            size="sm"
            disabled={loading}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
            刷新
          </Button>
        </div>
      </div>

      {/* 搜索和筛选 */}
      <Card className="border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Search className="h-5 w-5" />
            搜索和筛选
          </CardTitle>
        </CardHeader>
        <CardContent>
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
                className="px-3 py-2 border border-gray-200 dark:border-gray-700 rounded-md bg-white dark:bg-gray-800 text-sm"
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
          
          {isSearching && (
            <div className="mt-3 flex items-center gap-2">
              <Badge variant="secondary">
                搜索: "{searchKeyword}"
              </Badge>
              <Button 
                variant="ghost" 
                size="sm" 
                onClick={() => {
                  setSearchKeyword('');
                  setIsSearching(false);
                  fetchAllTools();
                }}
              >
                清除搜索
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* 主要内容 */}
      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="grid w-full grid-cols-3 mb-6">
          <TabsTrigger value="overview" className="flex items-center gap-2">
            <BarChart3 className="h-4 w-4" />
            概览统计
          </TabsTrigger>
          <TabsTrigger value="tools" className="flex items-center gap-2">
            <Bot className="h-4 w-4" />
            工具列表
          </TabsTrigger>
          <TabsTrigger value="categories" className="flex items-center gap-2">
            <Layers className="h-4 w-4" />
            分类浏览
          </TabsTrigger>
        </TabsList>

        {/* 概览统计 */}
        <TabsContent value="overview">
          <ToolsOverview overview={overview} loading={loading} />
        </TabsContent>

        {/* 工具列表 */}
        <TabsContent value="tools">
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-semibold">
                工具列表 ({filteredTools.length})
              </h3>
              <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                {selectedCategory !== 'all' && (
                  <Badge variant="secondary">
                    筛选: {selectedCategory}
                  </Badge>
                )}
              </div>
            </div>
            
            {loading ? (
              <div className="grid gap-4">
                {Array.from({ length: 3 }).map((_, i) => (
                  <Card key={i} className="animate-pulse border-gray-200 dark:border-gray-700">
                    <CardHeader>
                      <div className="h-6 bg-gray-200 rounded w-1/3"></div>
                      <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                    </CardHeader>
                    <CardContent>
                      <div className="h-4 bg-gray-200 rounded w-full mb-2"></div>
                      <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : filteredTools.length > 0 ? (
              <div className="grid gap-4">
                {filteredTools.map((tool) => (
                  <ToolDetailCard key={tool.name} tool={tool} />
                ))}
              </div>
            ) : (
              <Card className="border-gray-200 dark:border-gray-700">
                <CardContent className="flex flex-col items-center justify-center h-[200px]">
                  <Bot className="h-12 w-12 text-gray-400 mb-4" />
                  <p className="text-gray-500 dark:text-gray-400">
                    {isSearching ? '没有找到匹配的工具' : '暂无工具数据'}
                  </p>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>

        {/* 分类浏览 */}
        <TabsContent value="categories">
          <div className="space-y-6">
            {loading ? (
              <div className="grid gap-6">
                {Array.from({ length: 2 }).map((_, i) => (
                  <Card key={i} className="animate-pulse border-gray-200 dark:border-gray-700">
                    <CardHeader>
                      <div className="h-6 bg-gray-200 rounded w-1/4"></div>
                    </CardHeader>
                    <CardContent>
                      <div className="grid gap-4">
                        {Array.from({ length: 2 }).map((_, j) => (
                          <div key={j} className="h-20 bg-gray-200 rounded"></div>
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            ) : Object.keys(categorizedTools).length > 0 ? (
              Object.entries(categorizedTools)
                .sort(([,a], [,b]) => b.length - a.length)
                .map(([category, categoryTools]) => (
                  <Card key={category} className="border-gray-200 dark:border-gray-700">
                    <CardHeader>
                      <CardTitle className="flex items-center gap-2">
                        <Layers className="h-5 w-5" />
                        {category}
                        <Badge variant="outline">{categoryTools.length} 个工具</Badge>
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="grid gap-4">
                        {categoryTools.map((tool) => (
                          <ToolDetailCard key={tool.name} tool={tool} />
                        ))}
                      </div>
                    </CardContent>
                  </Card>
                ))
            ) : (
              <Card className="border-gray-200 dark:border-gray-700">
                <CardContent className="flex flex-col items-center justify-center h-[200px]">
                  <Layers className="h-12 w-12 text-gray-400 mb-4" />
                  <p className="text-gray-500 dark:text-gray-400">暂无分类数据</p>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
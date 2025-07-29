import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { 
  ChevronDown,
  ChevronRight,
  Bot,
  Hash,
  Clock,
  Settings,
  FileText,
  Code
} from "lucide-react";
import type { McpToolInfo } from "@/lib/agent-tools-api";

interface ToolCompactCardProps {
  tool: McpToolInfo;
  showDetails?: boolean;
}

export function ToolCompactCard({ tool, showDetails = false }: ToolCompactCardProps) {
  const [isExpanded, setIsExpanded] = useState(showDetails);

  const requiredParams = tool.parameters.filter(p => p.isRequired).length;
  const optionalParams = tool.parameters.length - requiredParams;

  return (
    <Card className="border-gray-200 dark:border-gray-700 hover:shadow-sm transition-shadow">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-start gap-3 flex-1">
            <div className="p-2 bg-gray-100 dark:bg-gray-800 rounded-lg shrink-0">
              <Bot className="h-4 w-4 text-gray-600 dark:text-gray-400" />
            </div>
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <CardTitle className="text-base font-semibold truncate">
                  {tool.name}
                </CardTitle>
                {tool.isAsync && (
                  <Badge variant="secondary" className="text-xs shrink-0">
                    <Clock className="h-3 w-3 mr-1" />
                    异步
                  </Badge>
                )}
              </div>
              <div className="flex items-center gap-2 mb-2">
                <Badge variant="outline" className="text-xs">
                  {tool.category}
                </Badge>
                <div className="flex items-center gap-1 text-xs text-gray-500">
                  <Hash className="h-3 w-3" />
                  {tool.parameters.length} 参数
                </div>
              </div>
              {tool.description && (
                <p className="text-sm text-gray-600 dark:text-gray-300 line-clamp-2">
                  {tool.description.length > 100 
                    ? `${tool.description.substring(0, 100)}...` 
                    : tool.description
                  }
                </p>
              )}
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsExpanded(!isExpanded)}
            className="shrink-0 ml-2"
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </Button>
        </div>
      </CardHeader>

      {isExpanded && (
        <CardContent className="pt-0">
          <div className="space-y-4">
            {/* 参数概览 */}
            {tool.parameters.length > 0 && (
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <Settings className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">参数概览</span>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                  {requiredParams > 0 && (
                    <div className="flex items-center justify-between p-2 bg-red-50 dark:bg-red-900/20 rounded-md">
                      <span className="text-sm font-medium">必需参数</span>
                      <Badge className="bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                        {requiredParams}
                      </Badge>
                    </div>
                  )}
                  {optionalParams > 0 && (
                    <div className="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-800/50 rounded-md">
                      <span className="text-sm font-medium">可选参数</span>
                      <Badge variant="secondary">
                        {optionalParams}
                      </Badge>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* 关键参数列表 */}
            {tool.parameters.length > 0 && (
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <Code className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">主要参数</span>
                </div>
                <div className="space-y-2 max-h-32 overflow-y-auto">
                  {tool.parameters.slice(0, 5).map((param, index) => (
                    <div key={index} className="flex items-center justify-between text-xs">
                      <div className="flex items-center gap-2">
                        <code className="font-mono bg-gray-100 dark:bg-gray-800 px-1 rounded">
                          {param.name}
                        </code>
                        {param.isRequired && (
                          <Badge variant="default" className="text-xs bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                            必需
                          </Badge>
                        )}
                      </div>
                      <Badge variant="outline" className="text-xs font-mono">
                        {param.type}
                      </Badge>
                    </div>
                  ))}
                  {tool.parameters.length > 5 && (
                    <div className="text-xs text-gray-500 text-center py-1">
                      还有 {tool.parameters.length - 5} 个参数...
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* 返回类型 */}
            <div className="flex items-center justify-between p-2 bg-blue-50 dark:bg-blue-900/20 rounded-md">
              <div className="flex items-center gap-2">
                <FileText className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                <span className="text-sm font-medium">返回类型</span>
              </div>
              <Badge variant="outline" className="font-mono text-xs">
                {tool.returnType}
              </Badge>
            </div>
          </div>
        </CardContent>
      )}
    </Card>
  );
}
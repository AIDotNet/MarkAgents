import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
// Note: Collapsible component might not be available, using manual state instead
import { 
  ChevronDown,
  ChevronRight,
  Bot,
  Type,
  Hash,
  Clock,
  FileText,
  Settings,
  Code,
  Braces
} from "lucide-react";
import type { McpToolInfo, ParameterInfo, TypeInfo } from "@/lib/agent-tools-api";

interface ToolDetailCardProps {
  tool: McpToolInfo;
  expanded?: boolean;
}

export function ToolDetailCard({ tool, expanded = false }: ToolDetailCardProps) {
  const [isExpanded, setIsExpanded] = useState(expanded);

  return (
    <Card className="border-gray-200 dark:border-gray-700">
      <CardHeader>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-gray-100 dark:bg-gray-800 rounded-lg">
              <Bot className="h-5 w-5 text-gray-600 dark:text-gray-400" />
            </div>
            <div>
              <CardTitle className="text-lg font-semibold">{tool.name}</CardTitle>
              <div className="flex items-center gap-2 mt-1">
                <Badge variant="outline" className="text-xs">
                  {tool.category}
                </Badge>
                {tool.isAsync && (
                  <Badge variant="secondary" className="text-xs bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                    <Clock className="h-3 w-3 mr-1" />
                    异步
                  </Badge>
                )}
              </div>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsExpanded(!isExpanded)}
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
          </Button>
        </div>
      </CardHeader>

      <CardContent>
        {/* 工具描述 */}
        {tool.description && (
          <div className="mb-4">
            <div className="flex items-center gap-2 mb-2">
              <FileText className="h-4 w-4 text-gray-500" />
              <span className="text-sm font-medium">描述</span>
            </div>
            <p className="text-sm text-gray-600 dark:text-gray-300 bg-gray-50 dark:bg-gray-800/50 p-3 rounded-md">
              {tool.description}
            </p>
          </div>
        )}

        {/* 基本信息 */}
        <div className="grid grid-cols-2 gap-4 mb-4">
          <div className="flex items-center gap-2">
            <Hash className="h-4 w-4 text-gray-500" />
            <span className="text-sm font-medium">参数数量:</span>
            <Badge variant="outline">{tool.parameters.length}</Badge>
          </div>
          <div className="flex items-center gap-2">
            <Type className="h-4 w-4 text-gray-500" />
            <span className="text-sm font-medium">返回类型:</span>
            <Badge variant="secondary" className="font-mono text-xs">
              {tool.returnType}
            </Badge>
          </div>
        </div>

        {/* 详细信息 */}
        {isExpanded && (
          <div className="space-y-4">
            {/* 参数详情 */}
            {tool.parameters.length > 0 && (
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <Settings className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">参数详情</span>
                </div>
                <div className="space-y-3">
                  {tool.parameters.map((param, index) => (
                    <ParameterCard key={index} parameter={param} />
                  ))}
                </div>
              </div>
            )}

            {/* 返回类型详情 */}
            {tool.returnTypeInfo && (
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <Code className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium">返回类型详情</span>
                </div>
                <TypeInfoCard typeInfo={tool.returnTypeInfo} />
              </div>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

interface ParameterCardProps {
  parameter: ParameterInfo;
}

function ParameterCard({ parameter }: ParameterCardProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <div className="border border-gray-200 dark:border-gray-700 rounded-lg p-3">
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-2">
          <span className="font-mono text-sm font-medium">{parameter.name}</span>
          <Badge 
            variant={parameter.isRequired ? "default" : "secondary"}
            className={parameter.isRequired 
              ? "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200" 
              : "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200"
            }
          >
            {parameter.isRequired ? "必需" : "可选"}
          </Badge>
          <Badge variant="outline" className="font-mono text-xs">
            {parameter.type}
          </Badge>
        </div>
        {parameter.isComplexType && parameter.typeInfo && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsExpanded(!isExpanded)}
          >
            {isExpanded ? (
              <ChevronDown className="h-3 w-3" />
            ) : (
              <ChevronRight className="h-3 w-3" />
            )}
          </Button>
        )}
      </div>

      {parameter.description && (
        <p className="text-xs text-gray-600 dark:text-gray-400 mb-2">
          {parameter.description}
        </p>
      )}

      {parameter.defaultValue !== undefined && (
        <div className="text-xs text-gray-500">
          默认值: <code className="bg-gray-100 dark:bg-gray-800 px-1 rounded">
            {JSON.stringify(parameter.defaultValue)}
          </code>
        </div>
      )}

      {parameter.isComplexType && parameter.typeInfo && isExpanded && (
        <div className="mt-3">
          <TypeInfoCard typeInfo={parameter.typeInfo} />
        </div>
      )}
    </div>
  );
}

interface TypeInfoCardProps {
  typeInfo: TypeInfo;
}

function TypeInfoCard({ typeInfo }: TypeInfoCardProps) {
  return (
    <div className="bg-gray-50 dark:bg-gray-800/50 rounded-lg p-3">
      <div className="flex items-center gap-2 mb-2">
        <Braces className="h-4 w-4 text-gray-500" />
        <span className="text-sm font-medium">{typeInfo.typeName}</span>
        {typeInfo.isEnum && (
          <Badge variant="outline" className="text-xs">枚举</Badge>
        )}
        {typeInfo.isArray && (
          <Badge variant="outline" className="text-xs">数组</Badge>
        )}
      </div>

      {typeInfo.isEnum && typeInfo.enumValues && (
        <div className="space-y-2">
          <div className="text-xs font-medium text-gray-600 dark:text-gray-400">枚举值:</div>
          <div className="flex flex-wrap gap-1">
            {typeInfo.enumValues.map((enumValue, index) => (
              <Badge key={index} variant="secondary" className="text-xs">
                {enumValue.name} ({enumValue.value})
              </Badge>
            ))}
          </div>
        </div>
      )}

      {typeInfo.properties && typeInfo.properties.length > 0 && (
        <div className="space-y-2">
          <div className="text-xs font-medium text-gray-600 dark:text-gray-400">属性:</div>
          <div className="space-y-1">
            {typeInfo.properties.map((prop, index) => (
              <div key={index} className="flex items-center justify-between text-xs">
                <span className="font-mono">{prop.name}</span>
                <div className="flex items-center gap-1">
                  <Badge variant="outline" className="text-xs">{prop.type}</Badge>
                  {prop.isRequired && (
                    <Badge variant="default" className="text-xs bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200">
                      必需
                    </Badge>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {typeInfo.isArray && typeInfo.elementType && (
        <div className="text-xs text-gray-600 dark:text-gray-400">
          元素类型: <code className="bg-white dark:bg-gray-700 px-1 rounded">{typeInfo.elementType}</code>
        </div>
      )}
    </div>
  );
}
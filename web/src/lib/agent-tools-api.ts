// AgentTools API 接口定义

/**
 * MCP工具信息
 */
export interface McpToolInfo {
  /** 方法名称 */
  name: string;
  /** 工具描述 */
  description: string;
  /** 方法参数信息 */
  parameters: ParameterInfo[];
  /** 返回类型名称 */
  returnType: string;
  /** 返回类型的详细信息 */
  returnTypeInfo?: TypeInfo;
  /** 是否为异步方法 */
  isAsync: boolean;
  /** 工具类别 */
  category: string;
}

/**
 * 参数信息
 */
export interface ParameterInfo {
  /** 参数名称 */
  name: string;
  /** 参数类型 */
  type: string;
  /** 参数描述 */
  description: string;
  /** 是否为必需参数 */
  isRequired: boolean;
  /** 默认值 */
  defaultValue?: any;
  /** 是否为复杂类型 */
  isComplexType: boolean;
  /** 复杂类型的详细信息 */
  typeInfo?: TypeInfo;
}

/**
 * 类型信息
 */
export interface TypeInfo {
  /** 类型名称 */
  typeName: string;
  /** 类型的完整名称 */
  fullTypeName: string;
  /** 是否为枚举类型 */
  isEnum: boolean;
  /** 枚举值 */
  enumValues?: EnumValueInfo[];
  /** 属性信息 */
  properties?: PropertyInfo[];
  /** 是否为数组类型 */
  isArray: boolean;
  /** 数组元素类型 */
  elementType?: string;
}

/**
 * 枚举值信息
 */
export interface EnumValueInfo {
  /** 枚举值名称 */
  name: string;
  /** 枚举值 */
  value: number;
  /** 枚举值描述 */
  description: string;
}

/**
 * 属性信息
 */
export interface PropertyInfo {
  /** 属性名称 */
  name: string;
  /** 属性类型 */
  type: string;
  /** 属性描述 */
  description: string;
  /** 是否为必需属性 */
  isRequired: boolean;
  /** 是否可为空 */
  isNullable: boolean;
}

/**
 * 工具概览信息
 */
export interface ToolOverviewInfo {
  /** 工具总数 */
  totalTools: number;
  /** 类别总数 */
  categories: number;
  /** 按类别统计的工具数量 */
  toolsByCategory: Record<string, number>;
  /** 异步工具数量 */
  asyncTools: number;
  /** 同步工具数量 */
  syncTools: number;
  /** 参数统计信息 */
  parameterStats: ParameterStatistics;
}

/**
 * 参数统计信息
 */
export interface ParameterStatistics {
  /** 总参数数量 */
  totalParameters: number;
  /** 必需参数数量 */
  requiredParameters: number;
  /** 可选参数数量 */
  optionalParameters: number;
  /** 复杂类型参数数量 */
  complexTypeParameters: number;
}

/**
 * API 响应基础接口
 */
export interface ApiResponse<T> {
  data?: T;
  error?: string;
  details?: string;
}

/**
 * AgentTools API 客户端类
 */
export class AgentToolsApi {
  private baseUrl: string;

  constructor(baseUrl: string = '/api/agent-tools') {
    this.baseUrl = baseUrl;
  }

  /**
   * 获取所有MCP工具的信息
   */
  async getAllTools(): Promise<McpToolInfo[]> {
    const response = await fetch(this.baseUrl);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || '获取工具信息失败');
    }
    return response.json();
  }

  /**
   * 根据工具名称获取特定工具的详细信息
   */
  async getToolByName(toolName: string): Promise<McpToolInfo> {
    const response = await fetch(`${this.baseUrl}/${encodeURIComponent(toolName)}`);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || '获取工具信息失败');
    }
    return response.json();
  }

  /**
   * 获取工具按类别分组的信息
   */
  async getToolsByCategory(): Promise<Record<string, McpToolInfo[]>> {
    const response = await fetch(`${this.baseUrl}/categories`);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || '获取分类工具信息失败');
    }
    return response.json();
  }

  /**
   * 获取工具概览统计信息
   */
  async getToolsOverview(): Promise<ToolOverviewInfo> {
    const response = await fetch(`${this.baseUrl}/overview`);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || '获取工具概览信息失败');
    }
    return response.json();
  }

  /**
   * 搜索工具
   */
  async searchTools(keyword: string): Promise<McpToolInfo[]> {
    const response = await fetch(`${this.baseUrl}/search?keyword=${encodeURIComponent(keyword)}`);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || '搜索工具失败');
    }
    return response.json();
  }
}

/**
 * 创建默认的 AgentTools API 实例
 */
export const agentToolsApi = new AgentToolsApi();

/**
 * 自定义 Hook：使用 AgentTools API
 */
export function useAgentTools() {
  const [tools, setTools] = useState<McpToolInfo[]>([]);
  const [overview, setOverview] = useState<ToolOverviewInfo | null>(null);
  const [categorizedTools, setCategorizedTools] = useState<Record<string, McpToolInfo[]>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchAllTools = async () => {
    setLoading(true);
    setError(null);
    try {
      const toolsData = await agentToolsApi.getAllTools();
      setTools(toolsData);
    } catch (err) {
      setError(err instanceof Error ? err.message : '获取工具信息失败');
    } finally {
      setLoading(false);
    }
  };

  const fetchToolsOverview = async () => {
    setLoading(true);
    setError(null);
    try {
      const overviewData = await agentToolsApi.getToolsOverview();
      setOverview(overviewData);
    } catch (err) {
      setError(err instanceof Error ? err.message : '获取工具概览失败');
    } finally {
      setLoading(false);
    }
  };

  const fetchToolsByCategory = async () => {
    setLoading(true);
    setError(null);
    try {
      const categorizedData = await agentToolsApi.getToolsByCategory();
      setCategorizedTools(categorizedData);
    } catch (err) {
      setError(err instanceof Error ? err.message : '获取分类工具信息失败');
    } finally {
      setLoading(false);
    }
  };

  const searchTools = async (keyword: string) => {
    setLoading(true);
    setError(null);
    try {
      const searchResults = await agentToolsApi.searchTools(keyword);
      setTools(searchResults);
    } catch (err) {
      setError(err instanceof Error ? err.message : '搜索工具失败');
    } finally {
      setLoading(false);
    }
  };

  return {
    tools,
    overview,
    categorizedTools,
    loading,
    error,
    fetchAllTools,
    fetchToolsOverview,
    fetchToolsByCategory,
    searchTools,
  };
}

// React import for useState
import { useState } from 'react';
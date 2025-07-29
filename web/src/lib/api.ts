interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  status: number
}

interface User {
  id: string
  email: string
  userName: string
}

interface AuthResponse {
  message: string
  token: string
  user: User
  isNewUser?: boolean
}

interface OAuthProvider {
  provider: string
  description?: string
}

interface OAuthAuthorizeResponse {
  authorizeUrl: string
  state: string
}

// MCP Tool interfaces
export interface McpTool {
  id: string;
  toolName: string;
  displayName: string;
  description: string;
  toolType: 'Tool' | 'Resource' | 'Prompt';
  category: string;
  isBuiltIn: boolean;
  securityLevel: 'Safe' | 'Moderate' | 'Dangerous' | 'Restricted';
  inputSchema?: string;
  outputSchema?: string;
  tags: string[];
  version: string;
  author: string;
}

export interface McpToolCategory {
  value: string;
  displayName: string;
  description: string;
}

export interface UserMcpToolConfig {
  id: string;
  toolName: string;
  isEnabled: boolean;
  configuration?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ConfigureMcpToolRequest {
  userKeyId: string;
  toolName: string;
  isEnabled: boolean;
  configuration?: string;
}

class ApiService {
  private baseURL: string = ''
  private defaultHeaders: Record<string, string> = {
    'Content-Type': 'application/json',
  }

  constructor() {
    // 可以从环境变量读取baseURL
    this.baseURL = ''
  }

  private getAuthToken(): string | null {
    return localStorage.getItem('auth_token')
  }

  private setAuthToken(token: string): void {
    localStorage.setItem('auth_token', token)
  }

  private removeAuthToken(): void {
    localStorage.removeItem('auth_token')
  }

  private getHeaders(includeAuth: boolean = true): HeadersInit {
    const headers: Record<string, string> = { ...this.defaultHeaders }

    if (includeAuth) {
      const token = this.getAuthToken()
      if (token) {
        headers['Authorization'] = `Bearer ${token}`
      }
    }

    return headers
  }

  private async handleResponse<T>(response: Response): Promise<ApiResponse<T>> {
    const contentType = response.headers.get('content-type')
    const isJson = contentType?.includes('application/json')

    if (!response.ok) {
      let errorMessage = '请求失败'

      if (isJson) {
        try {
          const errorData = await response.json()
          errorMessage = errorData.message || errorMessage
        } catch {
          // 忽略JSON解析错误
        }
      } else {
        errorMessage = response.statusText || errorMessage
      }

      // 如果是401未授权，清除token
      if (response.status === 401) {
        this.removeAuthToken()
        // 401错误由组件层面处理重定向
      }

      return {
        success: false,
        message: errorMessage,
        status: response.status
      }
    }

    let data: T | undefined
    if (isJson) {
      data = await response.json()
    }

    return {
      success: true,
      data,
      status: response.status
    }
  }

  private async request<T>(
    url: string,
    options: RequestInit = {},
    includeAuth: boolean = true
  ): Promise<ApiResponse<T>> {
    try {
      const fullUrl = url.startsWith('http') ? url : `${this.baseURL}${url}`

      const config: RequestInit = {
        ...options,
        headers: {
          ...this.getHeaders(includeAuth),
          ...options.headers,
        },
      }

      const response = await fetch(fullUrl, config)
      return this.handleResponse<T>(response)
    } catch (error) {
      console.error('API请求失败:', error)
      return {
        success: false,
        message: error instanceof Error ? error.message : '网络连接失败',
        status: 0
      }
    }
  }

  // GET请求
  async get<T>(url: string, includeAuth: boolean = true): Promise<ApiResponse<T>> {
    return this.request<T>(url, { method: 'GET' }, includeAuth)
  }

  // POST请求
  async post<T>(url: string, data?: any, includeAuth: boolean = true): Promise<ApiResponse<T>> {
    return this.request<T>(url, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    }, includeAuth)
  }

  // PUT请求
  async put<T>(url: string, data?: any, includeAuth: boolean = true): Promise<ApiResponse<T>> {
    return this.request<T>(url, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    }, includeAuth)
  }

  // DELETE请求
  async delete<T>(url: string, includeAuth: boolean = true): Promise<ApiResponse<T>> {
    return this.request<T>(url, { method: 'DELETE' }, includeAuth)
  }

  // 认证相关方法
  async login(email: string, password: string): Promise<ApiResponse<AuthResponse>> {
    const response = await this.post<AuthResponse>('/api/auth/login', {
      email,
      password
    }, false)

    if (response.success && response.data?.token) {
      this.setAuthToken(response.data.token)
    }

    return response
  }

  async sendVerificationCode(email: string, type: string = 'Registration'): Promise<ApiResponse<any>> {
    return this.post('/api/email/send-verification-code', {
      email,
      type
    }, false)
  }

  async register(email: string, userName: string, password: string, verificationCode: string): Promise<ApiResponse<any>> {
    return this.post('/api/auth/register', {
      email,
      userName,
      password,
      verificationCode
    }, false)
  }

  async refreshToken(): Promise<ApiResponse<{ token: string }>> {
    const currentToken = this.getAuthToken()
    if (!currentToken) {
      return {
        success: false,
        message: '没有有效的token',
        status: 401
      }
    }

    const response = await this.post<{ token: string }>('/api/auth/refresh', {
      token: currentToken
    }, false)

    if (response.success && response.data?.token) {
      this.setAuthToken(response.data.token)
    }

    return response
  }

  logout(): void {
    this.removeAuthToken()
  }

  isAuthenticated(): boolean {
    return !!this.getAuthToken()
  }


  async getOAuthAuthorizeUrl(provider: string, redirectUri?: string): Promise<ApiResponse<OAuthAuthorizeResponse>> {
    const params = new URLSearchParams()
    if (redirectUri) {
      params.append('redirectUri', redirectUri)
    }
    
    const url = `/api/oauth/authorize/${provider}${params.toString() ? '?' + params.toString() : ''}`
    return this.request(url, {
      method: 'GET',
    }, false)
  }

  async oauthCallback(provider: string, code: string, state: string, redirectUri?: string): Promise<ApiResponse<AuthResponse>> {
    const params = new URLSearchParams({
      code,
      state,
    })
    if (redirectUri) {
      params.append('redirectUri', redirectUri)
    }

    const response = await this.request<AuthResponse>(`/api/oauth/callback/${provider}?${params.toString()}`, {
      method: 'GET',
    }, false)

    if (response.success && response.data?.token) {
      this.setAuthToken(response.data.token)
    }

    return response
  }

  // MCP Tools APIs
  async getAvailableMcpTools(search?: string): Promise<ApiResponse<McpTool[]>> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    
    const url = `/api/mcp-tools/available${params.toString() ? '?' + params.toString() : ''}`;
    return this.get(url);
  }

  async getUserMcpToolConfig(userKeyId: string): Promise<ApiResponse<UserMcpToolConfig[]>> {
    return this.get(`/api/mcp-tools/user-config?userKeyId=${userKeyId}`);
  }

  async configureMcpTool(request: ConfigureMcpToolRequest): Promise<ApiResponse<{ success: boolean; message: string }>> {
    return this.post('/api/mcp-tools/user-config', request);
  }

  async toggleMcpTool(configId: string): Promise<ApiResponse<{ success: boolean; isEnabled: boolean; message: string }>> {
    return this.post(`/api/mcp-tools/user-config/${configId}/toggle`, {});
  }

  async initializeAgentTools(): Promise<ApiResponse<{ success: boolean; message: string }>> {
    return this.post('/api/mcp-tools/initialize-agent-tools', {});
  }

  async addCustomMcpTool(toolData: {
    toolName: string
    displayName: string
    description?: string
    category?: string
    version?: string
    author?: string
    iconUrl?: string
    documentationUrl?: string
    repositoryUrl?: string
    defaultConfiguration?: string
    requiredPermissions?: string
    tags?: string
  }): Promise<ApiResponse<any>> {
    return this.post('/api/mcp-tools/custom', toolData)
  }
}

// 导出单例实例
export const apiService = new ApiService()

// 导出类型
export type { 
  ApiResponse, 
  User, 
  AuthResponse, 
  OAuthProvider, 
  OAuthAuthorizeResponse
} 
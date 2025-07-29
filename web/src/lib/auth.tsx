import React, { createContext, useContext, useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiService, type User, type OAuthProvider } from './api'

interface AuthContextType {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (email: string, password: string) => Promise<{ success: boolean; message?: string }>
  sendVerificationCode: (email: string) => Promise<{ success: boolean; message?: string }>
  register: (email: string, userName: string, password: string, verificationCode: string) => Promise<{ success: boolean; message?: string }>
  logout: () => void
  refreshAuth: () => Promise<void>
  // OAuth相关方法
  oauthProviders: OAuthProvider[]
  getOAuthProviders: () => Promise<void>
  startOAuthLogin: (provider: string) => Promise<{ success: boolean; message?: string }>
  handleOAuthCallback: (provider: string, code: string, state: string) => Promise<{ success: boolean; message?: string }>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [oauthProviders, setOauthProviders] = useState<OAuthProvider[]>([])

  const isAuthenticated = !!user && apiService.isAuthenticated()

  // 验证当前token并获取用户信息
  const validateToken = async () => {
    if (!apiService.isAuthenticated()) {
      setIsLoading(false)
      return
    }

    try {
      // 这里我们需要一个获取当前用户信息的API
      // 暂时通过刷新token来验证
      const result = await apiService.refreshToken()
      if (result.success) {
        // 如果需要用户信息，这里应该调用获取用户信息的API
        // 目前我们从localStorage获取用户信息（如果有的话）
        const userInfo = localStorage.getItem('user_info')
        if (userInfo) {
          setUser(JSON.parse(userInfo))
        }
      } else {
        setUser(null)
        apiService.logout()
      }
    } catch (error) {
      console.error('Token validation failed:', error)
      setUser(null)
      apiService.logout()
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (email: string, password: string) => {
    try {
      setIsLoading(true)
      const result = await apiService.login(email, password)
      
      if (result.success && result.data) {
        setUser(result.data.user)
        // 保存用户信息到localStorage
        localStorage.setItem('user_info', JSON.stringify(result.data.user))
        return { success: true }
      } else {
        return { success: false, message: result.message || '登录失败' }
      }
    } catch (error) {
      return { success: false, message: '登录过程中发生错误' }
    } finally {
      setIsLoading(false)
    }
  }

  const sendVerificationCode = async (email: string) => {
    try {
      const result = await apiService.sendVerificationCode(email)
      
      if (result.success) {
        return { success: true, message: '验证码已发送，请检查您的邮箱' }
      } else {
        return { success: false, message: result.message || '验证码发送失败' }
      }
    } catch (error) {
      return { success: false, message: '验证码发送过程中发生错误' }
    }
  }

  const register = async (email: string, userName: string, password: string, verificationCode: string) => {
    try {
      setIsLoading(true)
      const result = await apiService.register(email, userName, password, verificationCode)
      
      if (result.success) {
        return { success: true, message: '注册成功，请登录' }
      } else {
        return { success: false, message: result.message || '注册失败' }
      }
    } catch (error) {
      return { success: false, message: '注册过程中发生错误' }
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    setUser(null)
    localStorage.removeItem('user_info')
    localStorage.removeItem('auth_token')
    // 不在这里处理重定向，由组件处理
  }

  const refreshAuth = async () => {
    await validateToken()
  }

  // OAuth相关方法实现
  const getOAuthProviders = useCallback(async () => {
    try {
      const result = await apiService.getOAuthProviders()
      if (result.success && result.data) {
        setOauthProviders(result.data)
      }
    } catch (error) {
      console.error('Failed to get OAuth providers:', error)
    }
  }, [])

  const startOAuthLogin = useCallback(async (provider: string): Promise<{ success: boolean; message?: string }> => {
    try {
      const redirectUri = `${window.location.origin}/oauth/callback/${provider}`
      const result = await apiService.getOAuthAuthorizeUrl(provider, redirectUri)
      
      if (result.success && result.data) {
        // 保存state到sessionStorage用于验证
        sessionStorage.setItem(`oauth_state_${provider}`, result.data.state)
        // 跳转到OAuth授权页面
        window.location.href = result.data.authorizeUrl
        return { success: true }
      } else {
        return { success: false, message: result.message || 'Failed to get authorization URL' }
      }
    } catch (error) {
      console.error('OAuth login failed:', error)
      return { success: false, message: 'OAuth login failed' }
    }
  }, [])

  const handleOAuthCallback = useCallback(async (provider: string, code: string, state: string): Promise<{ success: boolean; message?: string }> => {
    try {
      // 验证state
      const savedState = sessionStorage.getItem(`oauth_state_${provider}`)
      if (savedState !== state) {
        return { success: false, message: 'Invalid OAuth state' }
      }

      const redirectUri = `${window.location.origin}/oauth/callback/${provider}`
      const result = await apiService.oauthCallback(provider, code, state, redirectUri)
      
      if (result.success && result.data) {
        setUser(result.data.user)
        localStorage.setItem('user_info', JSON.stringify(result.data.user))
        
        // 清理state
        sessionStorage.removeItem(`oauth_state_${provider}`)
        
        return { 
          success: true, 
          message: result.data.isNewUser ? '注册成功并已自动登录' : '登录成功' 
        }
      } else {
        return { success: false, message: result.message || 'OAuth callback failed' }
      }
    } catch (error) {
      console.error('OAuth callback failed:', error)
      return { success: false, message: 'OAuth callback failed' }
    }
  }, [setUser])

  useEffect(() => {
    validateToken()
    getOAuthProviders()
  }, [])

  const value: AuthContextType = {
    user,
    isAuthenticated,
    isLoading,
    login,
    sendVerificationCode,
    register,
    logout,
    refreshAuth,
    oauthProviders,
    getOAuthProviders,
    startOAuthLogin,
    handleOAuthCallback,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

// 路由保护组件
export const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      navigate('/login', { replace: true })
    }
  }, [isAuthenticated, isLoading, navigate])

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
          <p className="mt-2 text-sm text-muted-foreground">加载中...</p>
        </div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return null
  }

  return <>{children}</>
}

// 公开路由组件（已登录用户不能访问）
export const PublicRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth()
  const navigate = useNavigate()

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate('/admin/dashboard', { replace: true })
    }
  }, [isAuthenticated, isLoading, navigate])

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
          <p className="mt-2 text-sm text-muted-foreground">加载中...</p>
        </div>
      </div>
    )
  }

  if (isAuthenticated) {
    return null
  }

  return <>{children}</>
} 
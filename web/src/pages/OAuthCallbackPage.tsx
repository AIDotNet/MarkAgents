import React, { useEffect, useState, useRef, useCallback } from 'react'
import { useParams, useSearchParams, useNavigate } from 'react-router-dom'
import { useAuth } from '@/lib/auth'
import { toast } from 'sonner'

export function OAuthCallbackPage() {
  const { provider } = useParams<{ provider: string }>()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const { handleOAuthCallback } = useAuth()
  
  const [status, setStatus] = useState<'processing' | 'success' | 'error'>('processing')
  const [message, setMessage] = useState('')
  const [isRetrying, setIsRetrying] = useState(false)
  
  // 使用ref来防止重复处理
  const hasProcessed = useRef(false)
  
  // 重试处理函数
  const retryCallback = useCallback(async () => {
    if (isRetrying || !provider) return
    
    setIsRetrying(true)
    setStatus('processing')
    setMessage('')
    
    // 重置处理状态，允许重新处理
    hasProcessed.current = false
    
    // 稍微延迟后重试
    setTimeout(() => {
      processCallback()
      setIsRetrying(false)
    }, 1000)
  }, [provider, isRetrying])
  
  // 使用useCallback包装processCallback，避免依赖项变化
  const processCallback = useCallback(async () => {
    // 防止重复处理
    if (hasProcessed.current) {
      return
    }
    
    hasProcessed.current = true

    if (!provider) {
      setStatus('error')
      setMessage('缺少OAuth提供商参数')
      toast.error('OAuth配置错误：缺少提供商参数')
      return
    }

    const code = searchParams.get('code')
    const state = searchParams.get('state')
    const error = searchParams.get('error')
    const errorDescription = searchParams.get('error_description')

    if (error) {
      const errorMsg = errorDescription || error || 'OAuth认证失败'
      setStatus('error')
      setMessage(`OAuth认证失败: ${errorMsg}`)
      toast.error(`OAuth认证失败: ${errorMsg}`)
      return
    }

    if (!code || !state) {
      setStatus('error')
      setMessage('OAuth回调参数不完整，请重新尝试登录')
      toast.error('OAuth回调参数不完整')
      return
    }

    try {
      const result = await handleOAuthCallback(provider, code, state)
      
      if (result.success) {
        setStatus('success')
        setMessage(result.message || '登录成功')
        toast.success(result.message || '登录成功')
        
        // 延迟跳转，让用户看到成功消息
        setTimeout(() => {
          navigate('/admin/dashboard', { replace: true })
        }, 2000)
      } else {
        setStatus('error')
        setMessage(result.message || 'OAuth登录失败')
        toast.error(result.message || 'OAuth登录失败')
      }
    } catch (error) {
      console.error('OAuth callback error:', error)
      setStatus('error')
      setMessage('网络错误，请检查网络连接后重试')
      toast.error('OAuth登录失败：网络错误')
    }
  }, [provider, searchParams, handleOAuthCallback, navigate])

  useEffect(() => {
    processCallback()
  }, [processCallback])

  useEffect(() => {
    // 如果失败，5秒后跳转回登录页面
    if (status === 'error') {
      const timer = setTimeout(() => {
        navigate('/login', { replace: true })
      }, 5000)
      
      return () => clearTimeout(timer)
    }
  }, [status, navigate])

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="max-w-md w-full space-y-8 p-8">
        <div className="text-center">
          {status === 'processing' && (
            <>
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
              <h2 className="mt-6 text-2xl font-bold text-foreground">
                正在处理OAuth登录...
              </h2>
              <p className="mt-2 text-sm text-muted-foreground">
                请稍候，我们正在验证您的身份
              </p>
              {isRetrying && (
                <p className="mt-1 text-xs text-blue-600 dark:text-blue-400">
                  正在重试...
                </p>
              )}
            </>
          )}
          
          {status === 'success' && (
            <>
              <div className="rounded-full h-12 w-12 bg-green-100 dark:bg-green-900/20 mx-auto flex items-center justify-center">
                <svg className="h-6 w-6 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              </div>
              <h2 className="mt-6 text-2xl font-bold text-foreground">
                登录成功！
              </h2>
              <p className="mt-2 text-sm text-muted-foreground">
                {message}
              </p>
              <p className="mt-2 text-xs text-muted-foreground">
                正在跳转到主页面...
              </p>
            </>
          )}
          
          {status === 'error' && (
            <>
              <div className="rounded-full h-12 w-12 bg-red-100 dark:bg-red-900/20 mx-auto flex items-center justify-center">
                <svg className="h-6 w-6 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </div>
              <h2 className="mt-6 text-2xl font-bold text-foreground">
                登录失败
              </h2>
              <p className="mt-2 text-sm text-muted-foreground">
                {message}
              </p>
              <div className="mt-4 space-y-2">
                <button 
                  onClick={retryCallback}
                  disabled={isRetrying}
                  className="px-4 py-2 text-sm bg-primary text-primary-foreground hover:bg-primary/90 rounded-md disabled:opacity-50"
                >
                  {isRetrying ? '重试中...' : '重试'}
                </button>
                <div>
                  <button 
                    onClick={() => navigate('/login', { replace: true })}
                    className="text-sm text-primary hover:text-primary/80 underline"
                  >
                    返回登录页面
                  </button>
                </div>
              </div>
              <p className="mt-2 text-xs text-muted-foreground">
                5秒后将自动跳转到登录页面
              </p>
            </>
          )}
        </div>
      </div>
    </div>
  )
} 
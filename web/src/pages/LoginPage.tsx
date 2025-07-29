import React, { useState } from 'react'
import { useAuth } from '@/lib/auth'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { toast } from 'sonner'
import { useNavigate } from 'react-router-dom'
import { ModeToggle } from '@/components/mode-toggle'

export function LoginPage() {
  const { login, register, sendVerificationCode, isLoading, oauthProviders, startOAuthLogin } = useAuth()
  const navigate = useNavigate()
  
  const [loginForm, setLoginForm] = useState({
    email: '',
    password: ''
  })
  
  const [registerForm, setRegisterForm] = useState({
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    verificationCode: ''
  })

  const [codeTimer, setCodeTimer] = useState(0)
  const [isCodeSending, setIsCodeSending] = useState(false)

  // 倒计时效果
  React.useEffect(() => {
    let interval: NodeJS.Timeout | null = null
    if (codeTimer > 0) {
      interval = setInterval(() => {
        setCodeTimer(codeTimer - 1)
      }, 1000)
    }
    return () => {
      if (interval) clearInterval(interval)
    }
  }, [codeTimer])

  const handleSendVerificationCode = async () => {
    if (!registerForm.email) {
      toast.error('请先输入邮箱地址')
      return
    }

    if (codeTimer > 0) {
      return
    }

    setIsCodeSending(true)
    const result = await sendVerificationCode(registerForm.email)
    
    if (result.success) {
      toast.success(result.message || '验证码已发送')
      setCodeTimer(60) // 60秒倒计时
    } else {
      toast.error(result.message || '验证码发送失败')
    }
    setIsCodeSending(false)
  }

  const handleOAuthLogin = async (provider: string) => {
    const result = await startOAuthLogin(provider)
    if (!result.success) {
      toast.error(result.message || 'OAuth登录失败')
    }
    // 成功的话会跳转到OAuth页面，所以这里不需要处理成功情况
  }

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!loginForm.email || !loginForm.password) {
      toast.error('请填写所有字段')
      return
    }

    const result = await login(loginForm.email, loginForm.password)
    
    if (result.success) {
      toast.success('登录成功')
      navigate('/admin/dashboard')
    } else {
      toast.error(result.message || '登录失败')
    }
  }

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!registerForm.email || !registerForm.userName || !registerForm.password || !registerForm.confirmPassword) {
      toast.error('请填写所有字段')
      return
    }

    if (registerForm.password !== registerForm.confirmPassword) {
      toast.error('密码确认不匹配')
      return
    }

    if (registerForm.password.length < 6) {
      toast.error('密码长度至少为6位')
      return
    }

    if (!registerForm.verificationCode) {
      toast.error('请输入邮箱验证码')
      return
    }

    const result = await register(registerForm.email, registerForm.userName, registerForm.password, registerForm.verificationCode)
    
    if (result.success) {
      toast.success(result.message || '注册成功')
      // 清空注册表单
      setRegisterForm({
        email: '',
        userName: '',
        password: '',
        confirmPassword: '',
        verificationCode: ''
      })
      // 切换到登录标签
      const loginTab = document.querySelector('[value="login"]') as HTMLElement
      loginTab?.click()
    } else {
      toast.error(result.message || '注册失败')
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Mark Agent</h1>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            智能代理管理平台
          </p>
        </div>
        
        <Tabs defaultValue="login" className="w-full">
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="login">登录</TabsTrigger>
            <TabsTrigger value="register">注册</TabsTrigger>
          </TabsList>
          
          <TabsContent value="login">
            <Card>
              <CardHeader>
                <CardTitle>登录账户</CardTitle>
                <CardDescription>
                  使用您的邮箱和密码登录
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleLogin} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="login-email">邮箱</Label>
                    <Input
                      id="login-email"
                      type="email"
                      placeholder="your@email.com"
                      value={loginForm.email}
                      onChange={(e) => setLoginForm({ ...loginForm, email: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="login-password">密码</Label>
                    <Input
                      id="login-password"
                      type="password"
                      placeholder="请输入密码"
                      value={loginForm.password}
                      onChange={(e) => setLoginForm({ ...loginForm, password: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <Button 
                    type="submit" 
                    className="w-full" 
                    disabled={isLoading}
                  >
                    {isLoading ? '登录中...' : '登录'}
                  </Button>
                </form>
                
                {/* OAuth登录分隔线 */}
                {oauthProviders.length > 0 && (
                  <>
                    <div className="relative my-4">
                      <div className="absolute inset-0 flex items-center">
                        <span className="w-full border-t" />
                      </div>
                      <div className="relative flex justify-center text-xs uppercase">
                        <span className="bg-background px-2 text-muted-foreground">
                          或使用第三方登录
                        </span>
                      </div>
                    </div>
                    
                    {/* OAuth登录按钮 */}
                    <div className="grid gap-2">
                      {oauthProviders.map((provider) => (
                        <Button
                          key={provider.provider}
                          variant="outline"
                          type="button"
                          className="w-full"
                          onClick={() => handleOAuthLogin(provider.provider)}
                          disabled={isLoading}
                        >
                          {provider.provider === 'github' && (
                            <svg className="mr-2 h-4 w-4" viewBox="0 0 24 24">
                              <path
                                fill="currentColor"
                                d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"
                              />
                            </svg>
                          )}
                          {provider.provider === 'gitee' && (
                            <svg className="mr-2 h-4 w-4" viewBox="0 0 24 24">
                              <path
                                fill="currentColor"
                                d="M12 0C5.374 0 0 5.373 0 12s5.374 12 12 12 12-5.373 12-12S18.626 0 12 0zm5.568 7.178l-2.427 2.427c-.542-.542-1.27-.839-2.045-.839s-1.503.297-2.045.839l-2.427-2.427c1.084-1.084 2.538-1.681 4.072-1.681s2.988.597 4.072 1.681zm-1.396 6.251c-.542.542-1.27.839-2.045.839s-1.503-.297-2.045-.839l-2.427 2.427c1.084 1.084 2.538 1.681 4.072 1.681s2.988-.597 4.072-1.681l-2.427-2.427z"
                              />
                            </svg>
                          )}
                          {provider.provider === 'google' && (
                            <svg className="mr-2 h-4 w-4" viewBox="0 0 24 24">
                              <path
                                fill="currentColor"
                                d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
                              />
                              <path
                                fill="currentColor"
                                d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
                              />
                              <path
                                fill="currentColor"
                                d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
                              />
                              <path
                                fill="currentColor"
                                d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
                              />
                            </svg>
                          )}
                          使用 {provider.description || provider.provider} 登录
                        </Button>
                      ))}
                    </div>
                  </>
                )}
              </CardContent>
            </Card>
          </TabsContent>
          
          <TabsContent value="register">
            <Card>
              <CardHeader>
                <CardTitle>创建账户</CardTitle>
                <CardDescription>
                  注册新的Mark Agent账户
                </CardDescription>
              </CardHeader>
              <CardContent>
                <form onSubmit={handleRegister} className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="register-email">邮箱</Label>
                    <Input
                      id="register-email"
                      type="email"
                      placeholder="your@email.com"
                      value={registerForm.email}
                      onChange={(e) => setRegisterForm({ ...registerForm, email: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="register-verification-code">邮箱验证码</Label>
                    <div className="flex space-x-2">
                      <Input
                        id="register-verification-code"
                        type="text"
                        placeholder="请输入6位验证码"
                        value={registerForm.verificationCode}
                        onChange={(e) => setRegisterForm({ ...registerForm, verificationCode: e.target.value })}
                        disabled={isLoading}
                        maxLength={6}
                        required
                        className="flex-1"
                      />
                      <Button
                        type="button"
                        variant="outline"
                        onClick={handleSendVerificationCode}
                        disabled={isCodeSending || codeTimer > 0 || !registerForm.email}
                        className="whitespace-nowrap"
                      >
                        {isCodeSending ? '发送中...' : codeTimer > 0 ? `${codeTimer}s` : '获取验证码'}
                      </Button>
                    </div>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="register-username">用户名</Label>
                    <Input
                      id="register-username"
                      type="text"
                      placeholder="请输入用户名"
                      value={registerForm.userName}
                      onChange={(e) => setRegisterForm({ ...registerForm, userName: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="register-password">密码</Label>
                    <Input
                      id="register-password"
                      type="password"
                      placeholder="至少6位密码"
                      value={registerForm.password}
                      onChange={(e) => setRegisterForm({ ...registerForm, password: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="register-confirm-password">确认密码</Label>
                    <Input
                      id="register-confirm-password"
                      type="password"
                      placeholder="再次输入密码"
                      value={registerForm.confirmPassword}
                      onChange={(e) => setRegisterForm({ ...registerForm, confirmPassword: e.target.value })}
                      disabled={isLoading}
                      required
                    />
                  </div>
                  <Button 
                    type="submit" 
                    className="w-full" 
                    disabled={isLoading}
                  >
                    {isLoading ? '注册中...' : '注册'}
                  </Button>
                </form>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  )
} 
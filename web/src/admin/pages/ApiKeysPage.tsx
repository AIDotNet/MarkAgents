import * as React from "react"
import { useState, useEffect } from "react"
import { IconPlus, IconKey, IconEdit, IconTrash, IconEye, IconEyeOff, IconCopy, IconSettings, IconTool } from "@tabler/icons-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Skeleton } from "@/components/ui/skeleton"
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Checkbox } from "@/components/ui/checkbox"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { toast } from "sonner"
import { apiService, type McpTool, type McpToolCategory, type UserMcpToolConfig } from "@/lib/api"

interface ApiKey {
  id: string
  name: string
  description?: string
  key: string
  isDefault: boolean
  status: 'Active' | 'Disabled' | 'Expired'
  usageCount: number
  lastUsedAt?: string
  expiresAt?: string
  createdAt: string
}

export default function ApiKeysPage() {
  const [apiKeys, setApiKeys] = useState<ApiKey[]>([])
  const [loading, setLoading] = useState(true)
  const [showKey, setShowKey] = useState<Record<string, boolean>>({})
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [editDialogOpen, setEditDialogOpen] = useState(false)
  const [editingKey, setEditingKey] = useState<ApiKey | null>(null)
  const [newKeyName, setNewKeyName] = useState('')
  const [newKeyDescription, setNewKeyDescription] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [selectedMcpTools, setSelectedMcpTools] = useState<string[]>([])
  const [availableToolsForForm, setAvailableToolsForForm] = useState<McpTool[]>([])
  const [mcpToolsDialogOpen, setMcpToolsDialogOpen] = useState(false)
  const [selectedApiKey, setSelectedApiKey] = useState<ApiKey | null>(null)
  const [availableMcpTools, setAvailableMcpTools] = useState<McpTool[]>([])
  const [mcpToolCategories, setMcpToolCategories] = useState<McpToolCategory[]>([])
  const [userMcpConfigs, setUserMcpConfigs] = useState<UserMcpToolConfig[]>([])
  const [loadingMcpTools, setLoadingMcpTools] = useState(false)
  const [selectedCategory, setSelectedCategory] = useState<string>('all')
  const [mcpSearchTerm, setMcpSearchTerm] = useState('')
  const [addCustomMcpDialogOpen, setAddCustomMcpDialogOpen] = useState(false)
  const [customMcpForm, setCustomMcpForm] = useState({
    toolName: '',
    displayName: '',
    description: '',
    category: '',
    version: '',
    author: '',
    iconUrl: '',
    documentationUrl: '',
    repositoryUrl: '',
    defaultConfiguration: '',
    requiredPermissions: '',
    tags: ''
  })

  useEffect(() => {
    loadApiKeys()
    loadAvailableTools()
  }, [])

  const loadAvailableTools = async () => {
    try {
      const response = await apiService.getAvailableMcpTools()
      if (response.success && response.data) {
        setAvailableToolsForForm(response.data)
      }
    } catch (error) {
      console.error('Failed to load available tools:', error)
    }
  }

  const loadApiKeys = async () => {
    try {
      setLoading(true)
      const response = await apiService.get('/api/user-keys')
      
      if (response.success && response.data) {
        setApiKeys(response.data as ApiKey[])
      } else {
        toast.error(response.message || '获取API密钥列表失败')
      }
    } catch (error) {
      console.error('Failed to load API keys:', error)
      toast.error('获取API密钥列表失败')
    } finally {
      setLoading(false)
    }
  }

  const toggleKeyVisibility = (keyId: string) => {
    setShowKey(prev => ({
      ...prev,
      [keyId]: !prev[keyId]
    }))
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('zh-CN')
  }

  const getStatusBadge = (status: string) => {
    const variants = {
      'Active': 'default',
      'Disabled': 'secondary',
      'Expired': 'destructive'
    } as const
    
    return (
      <Badge variant={variants[status as keyof typeof variants] || 'secondary'}>
        {status === 'Active' ? '活跃' : status === 'Disabled' ? '已禁用' : '已过期'}
      </Badge>
    )
  }

  const maskKey = (key: string) => {
    if (key.length < 8) return '***'
    return key.slice(0, 3) + '*'.repeat(Math.max(0, key.length - 7)) + key.slice(-4)
  }

  const handleCreateApiKey = async () => {
    if (!newKeyName.trim()) {
      toast.error('请输入密钥名称')
      return
    }

    try {
      setSubmitting(true)
      const response = await apiService.post('/api/user-keys', {
        name: newKeyName,
        description: newKeyDescription
      })
      
      if (response.success && response.data) {
        const newKeyId = (response.data as any).id
        
        // 配置选中的MCP工具
        if (selectedMcpTools.length > 0) {
          for (const toolName of selectedMcpTools) {
            try {
              await apiService.configureMcpTool({
                userKeyId: newKeyId,
                toolName,
                isEnabled: true
              })
            } catch (error) {
              console.error(`Failed to configure tool ${toolName}:`, error)
            }
          }
        }
        
        toast.success('API密钥创建成功')
        setCreateDialogOpen(false)
        setNewKeyName('')
        setNewKeyDescription('')
        setSelectedMcpTools([])
        // 重新加载列表
        await loadApiKeys()
      } else {
        toast.error(response.message || '创建API密钥失败')
      }
    } catch (error) {
      toast.error('创建API密钥失败')
    } finally {
      setSubmitting(false)
    }
  }

  const handleEditApiKey = async () => {
    if (!editingKey || !newKeyName.trim()) {
      toast.error('请输入密钥名称')
      return
    }

    try {
      setSubmitting(true)
      const response = await apiService.put(`/api/user-keys/${editingKey.id}`, {
        name: newKeyName,
        description: newKeyDescription
      })
      
      if (response.success) {
        // 更新MCP工具配置
        if (editingKey) {
          // 获取当前配置
          const currentConfigResponse = await apiService.getUserMcpToolConfig(editingKey.id)
          const currentTools = currentConfigResponse.success && currentConfigResponse.data ? 
            currentConfigResponse.data.map(config => config.toolName) : []
          
          // 找出需要启用和禁用的工具
          const toolsToEnable = selectedMcpTools.filter(tool => !currentTools.includes(tool))
          const toolsToDisable = currentTools.filter(tool => !selectedMcpTools.includes(tool))
          
          // 配置工具
          for (const toolName of toolsToEnable) {
            try {
              await apiService.configureMcpTool({
                userKeyId: editingKey.id,
                toolName,
                isEnabled: true
              })
            } catch (error) {
              console.error(`Failed to enable tool ${toolName}:`, error)
            }
          }
          
          for (const toolName of toolsToDisable) {
            try {
              await apiService.configureMcpTool({
                userKeyId: editingKey.id,
                toolName,
                isEnabled: false
              })
            } catch (error) {
              console.error(`Failed to disable tool ${toolName}:`, error)
            }
          }
        }
        
        toast.success('API密钥更新成功')
        setEditDialogOpen(false)
        setEditingKey(null)
        setNewKeyName('')
        setNewKeyDescription('')
        setSelectedMcpTools([])
        // 重新加载列表
        await loadApiKeys()
      } else {
        toast.error(response.message || '更新API密钥失败')
      }
    } catch (error) {
      toast.error('更新API密钥失败')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteApiKey = async (keyId: string) => {
    if (!confirm('确定要删除此API密钥吗？此操作不可撤销。')) {
      return
    }

    try {
      const response = await apiService.delete(`/api/user-keys/${keyId}`)
      
      if (response.success) {
        toast.success('API密钥删除成功')
        // 重新加载列表
        await loadApiKeys()
      } else {
        toast.error(response.message || '删除API密钥失败')
      }
    } catch (error) {
      toast.error('删除API密钥失败')
    }
  }

  const handleCopyKey = async (key: string) => {
    try {
      await navigator.clipboard.writeText(key)
      toast.success('API密钥已复制到剪贴板')
    } catch (error) {
      toast.error('复制失败')
    }
  }

  const openEditDialog = async (apiKey: ApiKey) => {
    setEditingKey(apiKey)
    setNewKeyName(apiKey.name)
    setNewKeyDescription(apiKey.description || '')
    
    // 加载当前API Key的MCP工具配置
    try {
      const response = await apiService.getUserMcpToolConfig(apiKey.id)
      if (response.success && response.data) {
        const enabledTools = response.data
          .filter(config => config.isEnabled)
          .map(config => config.toolName)
        setSelectedMcpTools(enabledTools)
      }
    } catch (error) {
      console.error('Failed to load MCP tools for key:', error)
      setSelectedMcpTools([])
    }
    
    setEditDialogOpen(true)
  }

  const openMcpToolsDialog = async (apiKey: ApiKey) => {
    setSelectedApiKey(apiKey)
    setMcpToolsDialogOpen(true)
    await loadMcpToolsData(apiKey.id)
  }

  const loadMcpToolsData = async (apiKeyId: string) => {
    try {
      setLoadingMcpTools(true)
      
      // 并行加载数据
      const [categoriesResponse, toolsResponse, userConfigResponse] = await Promise.all([
        apiService.getMcpToolCategories(),
        apiService.getAvailableMcpTools(),
        apiService.getUserMcpToolConfig(apiKeyId)
      ])

      if (categoriesResponse.success) {
        setMcpToolCategories(categoriesResponse.data || [])
      }

      if (toolsResponse.success) {
        setAvailableMcpTools(toolsResponse.data || [])
      }

      if (userConfigResponse.success) {
        setUserMcpConfigs(userConfigResponse.data || [])
      }
    } catch (error) {
      console.error('Failed to load MCP tools data:', error)
      toast.error('加载MCP工具数据失败')
    } finally {
      setLoadingMcpTools(false)
    }
  }

  const handleMcpToolToggle = async (tool: McpTool, enabled: boolean) => {
    if (!selectedApiKey) return

    try {
      const response = await apiService.configureMcpTool({
        userKeyId: selectedApiKey.id,
        toolName: tool.toolName,
        isEnabled: enabled
      })

      if (response.success) {
        toast.success(enabled ? '工具已启用' : '工具已禁用')
        // 重新加载用户配置
        await loadMcpToolsData(selectedApiKey.id)
      } else {
        toast.error(response.message || '操作失败')
      }
    } catch (error) {
      toast.error('操作失败')
    }
  }

  const isToolEnabled = (toolName: string) => {
    return userMcpConfigs.some(config => config.toolName === toolName && config.isEnabled)
  }

  const filteredMcpTools = availableMcpTools.filter(tool => {
    const categoryMatch = selectedCategory === 'all' || tool.category === selectedCategory
    const searchMatch = !mcpSearchTerm || 
      tool.displayName.toLowerCase().includes(mcpSearchTerm.toLowerCase()) ||
      tool.toolName.toLowerCase().includes(mcpSearchTerm.toLowerCase()) ||
      (tool.description && tool.description.toLowerCase().includes(mcpSearchTerm.toLowerCase()))
    
    return categoryMatch && searchMatch
  })

  const handleMcpToolSelect = (toolName: string, selected: boolean) => {
    setSelectedMcpTools(prev => {
      if (selected) {
        return [...prev, toolName]
      } else {
        return prev.filter(name => name !== toolName)
      }
    })
  }

  const resetForm = () => {
    setNewKeyName('')
    setNewKeyDescription('')
    setSelectedMcpTools([])
  }

  const handleCustomMcpFormChange = (field: string, value: string) => {
    setCustomMcpForm(prev => ({
      ...prev,
      [field]: value
    }))
  }

  const handleAddCustomMcp = async () => {
    // 验证必填字段
    if (!customMcpForm.toolName.trim() || !customMcpForm.displayName.trim()) {
      toast.error('请填写工具名称和显示名称')
      return
    }

    try {
      setSubmitting(true)
      
      // 准备标签数组
      const tagsArray = customMcpForm.tags ? 
        customMcpForm.tags.split(',').map(tag => tag.trim()).filter(Boolean) : 
        []
      
      // 调用后端API添加自定义MCP工具
      const response = await apiService.addCustomMcpTool({
        ...customMcpForm,
        tags: tagsArray.join(',')
      })
      
      if (!response.success) {
        toast.error(response.message || '添加自定义MCP工具失败')
        return
      }
      
      // 重置表单并关闭对话框
      setCustomMcpForm({
        toolName: '',
        displayName: '',
        description: '',
        category: '',
        version: '',
        author: '',
        iconUrl: '',
        documentationUrl: '',
        repositoryUrl: '',
        defaultConfiguration: '',
        requiredPermissions: '',
        tags: ''
      })
      setAddCustomMcpDialogOpen(false)
      
      toast.success('自定义MCP工具添加成功')
      
      // 如果当前有选中的API Key，重新加载工具数据
      if (selectedApiKey) {
        await loadMcpToolsData(selectedApiKey.id)
      }
    } catch (error) {
      toast.error('添加自定义MCP工具失败')
    } finally {
      setSubmitting(false)
    }
  }

  const resetCustomMcpForm = () => {
    setCustomMcpForm({
      toolName: '',
      displayName: '',
      description: '',
      category: '',
      version: '',
      author: '',
      iconUrl: '',
      documentationUrl: '',
      repositoryUrl: '',
      defaultConfiguration: '',
      requiredPermissions: '',
      tags: ''
    })
  }

  return (
    <div className="h-full max-h-full overflow-hidden flex flex-col">
      <div className="flex items-center justify-between pb-6 flex-shrink-0">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">API Keys</h1>
          <p className="text-muted-foreground">
            管理您的API密钥，用于访问MCP工具和服务
          </p>
        </div>
        <Dialog open={createDialogOpen} onOpenChange={setCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <IconPlus className="mr-2 h-4 w-4" />
              创建密钥
            </Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-[425px]">
            <DialogHeader>
              <DialogTitle>创建新的API密钥</DialogTitle>
              <DialogDescription>
                创建一个新的API密钥来访问MCP工具和服务。
              </DialogDescription>
            </DialogHeader>
            <Tabs defaultValue="basic" className="w-full">
              <TabsList className="grid w-full grid-cols-2">
                <TabsTrigger value="basic">基本信息</TabsTrigger>
                <TabsTrigger value="tools">MCP工具</TabsTrigger>
              </TabsList>
              
              <TabsContent value="basic" className="space-y-4">
                <div className="grid gap-4">
                  <div className="grid grid-cols-4 items-center gap-4">
                    <Label htmlFor="name" className="text-right">
                      名称
                    </Label>
                    <Input
                      id="name"
                      value={newKeyName}
                      onChange={(e) => setNewKeyName(e.target.value)}
                      className="col-span-3"
                      placeholder="为您的密钥输入一个名称"
                    />
                  </div>
                  <div className="grid grid-cols-4 items-center gap-4">
                    <Label htmlFor="description" className="text-right">
                      描述
                    </Label>
                    <Input
                      id="description"
                      value={newKeyDescription}
                      onChange={(e) => setNewKeyDescription(e.target.value)}
                      className="col-span-3"
                      placeholder="可选的描述信息"
                    />
                  </div>
                </div>
              </TabsContent>
              
              <TabsContent value="tools" className="space-y-4">
                <div className="space-y-4 max-h-64 overflow-y-auto">
                  <Label>选择要启用的MCP工具</Label>
                  {availableToolsForForm.map((tool) => (
                    <div key={tool.id} className="flex items-start space-x-3 p-3 border rounded-lg">
                      <Checkbox
                        checked={selectedMcpTools.includes(tool.toolName)}
                        onCheckedChange={(checked) => 
                          handleMcpToolSelect(tool.toolName, Boolean(checked))
                        }
                      />
                      <div className="flex-1 space-y-1">
                        <div className="flex items-center space-x-2">
                          <h4 className="font-medium text-sm">{tool.displayName}</h4>
                          {tool.isBuiltIn && (
                            <Badge variant="outline" className="text-xs">内置</Badge>
                          )}
                        </div>
                        {tool.description && (
                          <p className="text-xs text-muted-foreground">{tool.description}</p>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </TabsContent>
            </Tabs>
            <DialogFooter>
              <Button variant="outline" onClick={() => {
                setCreateDialogOpen(false)
                resetForm()
              }}>
                取消
              </Button>
              <Button onClick={handleCreateApiKey} disabled={submitting}>
                {submitting ? '创建中...' : '创建密钥'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>

      <Card className="flex-1 flex flex-col overflow-hidden">
        <CardHeader className="flex-shrink-0">
          <CardTitle>API密钥列表</CardTitle>
          <CardDescription>
            您当前拥有 {apiKeys.length} 个API密钥
          </CardDescription>
        </CardHeader>
        <CardContent className="flex-1 overflow-auto">
          {loading ? (
            <div className="space-y-4">
              {[...Array(3)].map((_, i) => (
                <div key={i} className="flex items-center space-x-4">
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-4 w-60" />
                  <Skeleton className="h-4 w-16" />
                  <Skeleton className="h-4 w-24" />
                </div>
              ))}
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>名称</TableHead>
                  <TableHead>密钥</TableHead>
                  <TableHead>状态</TableHead>
                  <TableHead>使用次数</TableHead>
                  <TableHead>最后使用</TableHead>
                  <TableHead>创建时间</TableHead>
                  <TableHead className="text-right">操作</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {apiKeys.map((apiKey) => (
                  <TableRow key={apiKey.id}>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <IconKey className="h-4 w-4 text-muted-foreground" />
                        <div>
                          <div className="font-medium">{apiKey.name}</div>
                          {apiKey.description && (
                            <div className="text-sm text-muted-foreground">
                              {apiKey.description}
                            </div>
                          )}
                          {apiKey.isDefault && (
                            <Badge variant="outline" className="text-xs">
                              默认
                            </Badge>
                          )}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        <code className="bg-muted px-2 py-1 rounded text-sm">
                          {showKey[apiKey.id] ? apiKey.key : maskKey(apiKey.key)}
                        </code>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => toggleKeyVisibility(apiKey.id)}
                        >
                          {showKey[apiKey.id] ? (
                            <IconEyeOff className="h-4 w-4" />
                          ) : (
                            <IconEye className="h-4 w-4" />
                          )}
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleCopyKey(apiKey.key)}
                        >
                          <IconCopy className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                    <TableCell>{getStatusBadge(apiKey.status)}</TableCell>
                    <TableCell>{apiKey.usageCount.toLocaleString()}</TableCell>
                    <TableCell>
                      {apiKey.lastUsedAt ? formatDate(apiKey.lastUsedAt) : '从未使用'}
                    </TableCell>
                    <TableCell>{formatDate(apiKey.createdAt)}</TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end space-x-2">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          onClick={() => openMcpToolsDialog(apiKey)}
                          title="配置MCP工具"
                        >
                          <IconSettings className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm"
                          onClick={() => openEditDialog(apiKey)}
                          title="编辑密钥"
                        >
                          <IconEdit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm"
                          onClick={() => handleDeleteApiKey(apiKey.id)}
                          title="删除密钥"
                        >
                          <IconTrash className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* 编辑对话框 */}
      <Dialog open={editDialogOpen} onOpenChange={setEditDialogOpen}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>编辑API密钥</DialogTitle>
            <DialogDescription>
              修改API密钥的名称和描述信息。
            </DialogDescription>
          </DialogHeader>
          <Tabs defaultValue="basic" className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="basic">基本信息</TabsTrigger>
              <TabsTrigger value="tools">MCP工具</TabsTrigger>
            </TabsList>
            
            <TabsContent value="basic" className="space-y-4">
              <div className="grid gap-4">
                <div className="grid grid-cols-4 items-center gap-4">
                  <Label htmlFor="edit-name" className="text-right">
                    名称
                  </Label>
                  <Input
                    id="edit-name"
                    value={newKeyName}
                    onChange={(e) => setNewKeyName(e.target.value)}
                    className="col-span-3"
                    placeholder="为您的密钥输入一个名称"
                  />
                </div>
                <div className="grid grid-cols-4 items-center gap-4">
                  <Label htmlFor="edit-description" className="text-right">
                    描述
                  </Label>
                  <Input
                    id="edit-description"
                    value={newKeyDescription}
                    onChange={(e) => setNewKeyDescription(e.target.value)}
                    className="col-span-3"
                    placeholder="可选的描述信息"
                  />
                </div>
              </div>
            </TabsContent>
            
            <TabsContent value="tools" className="space-y-4">
              <div className="space-y-4 max-h-64 overflow-y-auto">
                <Label>选择要启用的MCP工具</Label>
                {availableToolsForForm.map((tool) => (
                  <div key={tool.id} className="flex items-start space-x-3 p-3 border rounded-lg">
                    <Checkbox
                      checked={selectedMcpTools.includes(tool.toolName)}
                      onCheckedChange={(checked) => 
                        handleMcpToolSelect(tool.toolName, Boolean(checked))
                      }
                    />
                    <div className="flex-1 space-y-1">
                      <div className="flex items-center space-x-2">
                        <h4 className="font-medium text-sm">{tool.displayName}</h4>
                        {tool.isBuiltIn && (
                          <Badge variant="outline" className="text-xs">内置</Badge>
                        )}
                      </div>
                      {tool.description && (
                        <p className="text-xs text-muted-foreground">{tool.description}</p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </TabsContent>
          </Tabs>
          <DialogFooter>
            <Button variant="outline" onClick={() => {
              setEditDialogOpen(false)
              resetForm()
              setEditingKey(null)
            }}>
              取消
            </Button>
            <Button onClick={handleEditApiKey} disabled={submitting}>
              {submitting ? '保存中...' : '保存更改'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* MCP工具配置对话框 */}
      <Dialog open={mcpToolsDialogOpen} onOpenChange={setMcpToolsDialogOpen}>
        <DialogContent className="sm:max-w-[800px] max-h-[80vh] overflow-hidden">
          <DialogHeader>
            <DialogTitle>配置MCP工具</DialogTitle>
            <DialogDescription>
              为API密钥 "{selectedApiKey?.name}" 配置可用的MCP工具
            </DialogDescription>
          </DialogHeader>
          
          <div className="flex flex-col space-y-4 overflow-hidden">
            {/* 搜索和筛选 */}
            <div className="flex space-x-4">
              <div className="flex-1">
                <Input
                  placeholder="搜索工具..."
                  value={mcpSearchTerm}
                  onChange={(e) => setMcpSearchTerm(e.target.value)}
                />
              </div>
              <Select value={selectedCategory} onValueChange={setSelectedCategory}>
                <SelectTrigger className="w-48">
                  <SelectValue placeholder="选择分类" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">所有分类</SelectItem>
                  {mcpToolCategories.map((category) => (
                    <SelectItem key={category.value} value={category.value}>
                      {category.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* 工具列表 */}
            <div className="flex-1 overflow-y-auto max-h-96 border rounded-lg">
              {loadingMcpTools ? (
                <div className="p-8 text-center">
                  <div className="space-y-4">
                    {[...Array(3)].map((_, i) => (
                      <div key={i} className="flex items-center space-x-4">
                        <Skeleton className="h-4 w-4" />
                        <Skeleton className="h-4 w-40" />
                        <Skeleton className="h-4 w-60" />
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <div className="p-4 space-y-4">
                  {filteredMcpTools.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">
                      没有找到匹配的工具
                    </div>
                  ) : (
                    filteredMcpTools.map((tool) => (
                      <div
                        key={tool.id}
                        className="flex items-start justify-between p-4 border rounded-lg hover:bg-muted/50"
                      >
                        <div className="flex-1 space-y-1">
                          <div className="flex items-center space-x-2">
                            <h4 className="font-medium">{tool.displayName}</h4>
                            {tool.isBuiltIn && (
                              <Badge variant="outline" className="text-xs">
                                内置
                              </Badge>
                            )}
                            {tool.category && (
                              <Badge variant="secondary" className="text-xs">
                                {mcpToolCategories.find(c => c.value === tool.category)?.name || tool.category}
                              </Badge>
                            )}
                          </div>
                          {tool.description && (
                            <p className="text-sm text-muted-foreground">
                              {tool.description}
                            </p>
                          )}
                          <div className="flex items-center space-x-4 text-xs text-muted-foreground">
                            {tool.version && <span>版本: {tool.version}</span>}
                            {tool.author && <span>作者: {tool.author}</span>}
                            <span>使用次数: {tool.totalUsageCount}</span>
                          </div>
                        </div>
                        <div className="ml-4">
                          <Checkbox
                            checked={isToolEnabled(tool.toolName)}
                            onCheckedChange={(checked) => 
                              handleMcpToolToggle(tool, Boolean(checked))
                            }
                          />
                        </div>
                      </div>
                    ))
                  )}
                </div>
              )}
            </div>
          </div>

          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setAddCustomMcpDialogOpen(true)}
              className="mr-auto"
            >
              <IconTool className="mr-2 h-4 w-4" />
              添加自定义工具
            </Button>
            <Button variant="outline" onClick={() => setMcpToolsDialogOpen(false)}>
              关闭
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* 添加自定义MCP工具对话框 */}
      <Dialog open={addCustomMcpDialogOpen} onOpenChange={(open) => {
        setAddCustomMcpDialogOpen(open)
        if (!open) resetCustomMcpForm()
      }}>
        <DialogContent className="sm:max-w-[600px] max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>添加自定义MCP工具</DialogTitle>
            <DialogDescription>
              添加您自己的MCP工具到系统中，使其可以被配置和使用
            </DialogDescription>
          </DialogHeader>
          
          <Tabs defaultValue="basic" className="w-full">
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="basic">基本信息</TabsTrigger>
              <TabsTrigger value="advanced">高级配置</TabsTrigger>
            </TabsList>
            
            <TabsContent value="basic" className="space-y-4">
              <div className="grid gap-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="custom-tool-name">工具名称 *</Label>
                    <Input
                      id="custom-tool-name"
                      value={customMcpForm.toolName}
                      onChange={(e) => handleCustomMcpFormChange('toolName', e.target.value)}
                      placeholder="例如: my-custom-tool"
                      className="mt-1"
                    />
                  </div>
                  <div>
                    <Label htmlFor="custom-display-name">显示名称 *</Label>
                    <Input
                      id="custom-display-name"
                      value={customMcpForm.displayName}
                      onChange={(e) => handleCustomMcpFormChange('displayName', e.target.value)}
                      placeholder="例如: 我的自定义工具"
                      className="mt-1"
                    />
                  </div>
                </div>
                
                <div>
                  <Label htmlFor="custom-description">描述</Label>
                  <Input
                    id="custom-description"
                    value={customMcpForm.description}
                    onChange={(e) => handleCustomMcpFormChange('description', e.target.value)}
                    placeholder="工具的详细描述"
                    className="mt-1"
                  />
                </div>
                
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="custom-category">分类</Label>
                    <Select 
                      value={customMcpForm.category} 
                      onValueChange={(value) => handleCustomMcpFormChange('category', value)}
                    >
                      <SelectTrigger className="mt-1">
                        <SelectValue placeholder="选择分类" />
                      </SelectTrigger>
                      <SelectContent>
                        {mcpToolCategories.map((category) => (
                          <SelectItem key={category.value} value={category.value}>
                            {category.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="custom-version">版本</Label>
                    <Input
                      id="custom-version"
                      value={customMcpForm.version}
                      onChange={(e) => handleCustomMcpFormChange('version', e.target.value)}
                      placeholder="例如: 1.0.0"
                      className="mt-1"
                    />
                  </div>
                </div>
                
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="custom-author">作者</Label>
                    <Input
                      id="custom-author"
                      value={customMcpForm.author}
                      onChange={(e) => handleCustomMcpFormChange('author', e.target.value)}
                      placeholder="工具作者"
                      className="mt-1"
                    />
                  </div>
                  <div>
                    <Label htmlFor="custom-icon-url">图标URL</Label>
                    <Input
                      id="custom-icon-url"
                      value={customMcpForm.iconUrl}
                      onChange={(e) => handleCustomMcpFormChange('iconUrl', e.target.value)}
                      placeholder="https://example.com/icon.png"
                      className="mt-1"
                    />
                  </div>
                </div>
              </div>
            </TabsContent>
            
            <TabsContent value="advanced" className="space-y-4">
              <div className="grid gap-4">
                <div>
                  <Label htmlFor="custom-docs-url">文档URL</Label>
                  <Input
                    id="custom-docs-url"
                    value={customMcpForm.documentationUrl}
                    onChange={(e) => handleCustomMcpFormChange('documentationUrl', e.target.value)}
                    placeholder="https://example.com/docs"
                    className="mt-1"
                  />
                </div>
                
                <div>
                  <Label htmlFor="custom-repo-url">仓库URL</Label>
                  <Input
                    id="custom-repo-url"
                    value={customMcpForm.repositoryUrl}
                    onChange={(e) => handleCustomMcpFormChange('repositoryUrl', e.target.value)}
                    placeholder="https://github.com/user/repo"
                    className="mt-1"
                  />
                </div>
                
                <div>
                  <Label htmlFor="custom-tags">标签</Label>
                  <Input
                    id="custom-tags"
                    value={customMcpForm.tags}
                    onChange={(e) => handleCustomMcpFormChange('tags', e.target.value)}
                    placeholder="用逗号分隔，例如: utility,file,data"
                    className="mt-1"
                  />
                </div>
                
                <div>
                  <Label htmlFor="custom-config">默认配置 (JSON)</Label>
                  <Input
                    id="custom-config"
                    value={customMcpForm.defaultConfiguration}
                    onChange={(e) => handleCustomMcpFormChange('defaultConfiguration', e.target.value)}
                    placeholder='{"key": "value"}'
                    className="mt-1"
                  />
                </div>
                
                <div>
                  <Label htmlFor="custom-permissions">所需权限 (JSON)</Label>
                  <Input
                    id="custom-permissions"
                    value={customMcpForm.requiredPermissions}
                    onChange={(e) => handleCustomMcpFormChange('requiredPermissions', e.target.value)}
                    placeholder='["read", "write"]'
                    className="mt-1"
                  />
                </div>
              </div>
            </TabsContent>
          </Tabs>

          <DialogFooter>
            <Button 
              variant="outline" 
              onClick={() => setAddCustomMcpDialogOpen(false)}
            >
              取消
            </Button>
            <Button 
              onClick={handleAddCustomMcp} 
              disabled={submitting || !customMcpForm.toolName.trim() || !customMcpForm.displayName.trim()}
            >
              {submitting ? '添加中...' : '添加工具'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
} 
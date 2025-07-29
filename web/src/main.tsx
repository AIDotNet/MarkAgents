import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'sonner'
import './index.css'

import { ThemeProvider } from '@/components/theme-provider'
import { AuthProvider, ProtectedRoute, PublicRoute } from '@/lib/auth'
import { LoginPage } from '@/pages/LoginPage'
import { HomePage } from '@/pages/HomePage'
import { OAuthCallbackPage } from '@/pages/OAuthCallbackPage'
import { MCPIntegrationPage } from '@/pages/docs/MCPIntegrationPage'
import AdminLayout from '@/admin/layouts'
import DashboardPage from '@/admin/pages/DashboardPage'
import ApiKeysPage from '@/admin/pages/ApiKeysPage'

function App() {
  return (
    <ThemeProvider defaultTheme="system" storageKey="mark-agent-theme">
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route 
              path="/login" 
              element={
                <PublicRoute>
                  <LoginPage />
                </PublicRoute>
              } 
            />
            
            {/* OAuth回调路由 */}
            <Route 
              path="/oauth/callback/:provider" 
              element={<OAuthCallbackPage />}
            />
            
            {/* 文档路由 */}
            <Route 
              path="/docs/mcp-integration" 
              element={<MCPIntegrationPage />}
            />
            
            {/* 受保护的管理员路由 */}
            <Route 
              path="/admin" 
              element={
                <ProtectedRoute>
                  <AdminLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="/admin/dashboard" replace />} />
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="api-keys" element={<ApiKeysPage />} />
            </Route>

            {/* 其他路由重定向到首页 */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
          <Toaster position="top-right" />
        </BrowserRouter>
      </AuthProvider>
    </ThemeProvider>
  )
}

createRoot(document.getElementById('root')!).render(
  <App />
)

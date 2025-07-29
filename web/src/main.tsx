import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'sonner'
import './index.css'

import { ThemeProvider } from '@/components/theme-provider'
import { HomePage } from '@/pages/HomePage'
import { MCPIntegrationPage } from '@/pages/docs/MCPIntegrationPage'
import { ToolsPage } from '@/pages/ToolsPage'


function App() {
  return (
    <ThemeProvider defaultTheme="system" storageKey="mark-agent-theme">
      
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<HomePage />} />
            
            
            {/* 工具管理路由 */}
            <Route path="/tools" element={<ToolsPage />} />
            
            {/* 文档路由 */}
            <Route 
              path="/docs/mcp-integration" 
              element={<MCPIntegrationPage />}
            />
            
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
          <Toaster position="top-right" />
        </BrowserRouter>
      
    </ThemeProvider>
  )
}

createRoot(document.getElementById('root')!).render(
  <App />
)

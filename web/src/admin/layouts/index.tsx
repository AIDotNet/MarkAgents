import { AppSidebar } from "@/components/app-sidebar"
import { AdminHeader } from "@/components/admin-header"
import {
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar"

import { Outlet } from "react-router-dom"

export default function AdminLayout() {
  return (
    <div className="h-screen overflow-hidden">
      <SidebarProvider
        style={
          {
            "--sidebar-width": "calc(var(--spacing) * 72)",
            "--header-height": "calc(var(--spacing) * 12)",
          } as React.CSSProperties
        }
        className="h-full"
      >
        <AppSidebar variant="inset" />
        <SidebarInset className="flex flex-col h-full">
          <AdminHeader />
          <div className="flex-1 overflow-auto p-4">
            <Outlet />
          </div>
        </SidebarInset>
      </SidebarProvider>
    </div>
  )
}

import { ToolsDashboard } from '@/components/agent-tools/tools-dashboard';

export function ToolsPage() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* 主要内容 */}
      <main className="container mx-auto px-4 py-8">
        <ToolsDashboard />
      </main>
    </div>
  );
}
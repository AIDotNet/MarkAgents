# MCP 集成指南

## 简介

MarkAgent 是一个基于 Model Context Protocol (MCP) 的智能代理管理平台，为 AI 开发环境提供强大的工具支持。本文档将指导你如何将 MarkAgent 集成到不同的 IDE 和 AI 客户端中。

## 功能特性

### 🔧 TodoWrite 工具
- 智能任务管理和待办事项工具
- 支持优先级设置（High、Medium、Low、Urgent）
- 状态跟踪（Pending、InProgress、Completed）
- 实时控制台显示和进度更新

### 📊 实时统计
- 详细的工具使用统计
- 客户端连接监控
- 性能指标分析
- 多维度数据可视化

### ⚙️ 灵活配置
- 支持多种 IDE 和 AI 客户端
- HTTP 传输协议
- 标准 MCP 协议兼容
- 即插即用配置

## 快速开始

### 1. 选择你的 IDE
根据你使用的开发环境选择对应的配置方式

### 2. 复制配置代码
复制下方提供的 JSON 配置到相应的配置文件中

### 3. 重启 IDE
重启 IDE 让配置生效，开始使用 MarkAgent 工具

## 配置说明

### Cursor IDE

将以下配置添加到 Cursor 的 MCP 设置中：

\`\`\`json
{
  "mcpServers": {
    "todo": {
      "url": "https://agent.mark-chat.chat/mcp"
    }
  }
}
\`\`\`

**配置路径：** Cursor → 设置 → MCP Servers → 添加新服务器

### GitHub Copilot (VS Code)

在 VS Code 中为 GitHub Copilot 配置 MCP 服务器：

\`\`\`json
{
  "servers": {
    "todo": {
      "url": "https://agent.mark-chat.chat/mcp",
      "type": "http"
    }
  }
}
\`\`\`

**配置文件：** \`~/.vscode/mcp-servers.json\` 或工作区的 \`.vscode/settings.json\`

### Claude Desktop

为 Claude Desktop 应用配置 MCP 服务器：

\`\`\`json
{
  "mcpServers": {
    "markagent": {
      "command": "node",
      "args": ["/path/to/markagent-mcp-server/dist/index.js"],
      "env": {
        "MARKAGENT_SERVER_URL": "https://agent.mark-chat.chat/mcp"
      }
    }
  }
}
\`\`\`

**配置文件路径：**
- **macOS**: \`~/Library/Application Support/Claude/claude_desktop_config.json\`
- **Windows**: \`%APPDATA%\\Claude\\claude_desktop_config.json\`
- **Linux**: \`~/.config/claude/claude_desktop_config.json\`

### Continue (VS Code 插件)

如果你使用 Continue 插件，可以在 \`config.json\` 中添加：

\`\`\`json
{
  "mcpServers": [
    {
      "name": "markagent",
      "serverUrl": "https://agent.mark-chat.chat/mcp",
      "transport": "http"
    }
  ]
}
\`\`\`

## 使用示例

### TodoWrite 工具使用

配置完成后，你可以在 AI 对话中使用以下指令：

\`\`\`
请帮我创建一个任务列表：
1. 完成项目文档
2. 代码审查
3. 部署到生产环境
\`\`\`

AI 会自动调用 TodoWrite 工具来管理你的任务。

### 任务状态更新

\`\`\`
请将"完成项目文档"标记为已完成，并将"代码审查"设置为进行中
\`\`\`

### 优先级设置

\`\`\`
请创建一个高优先级任务：修复生产环境bug
\`\`\`

## 验证配置

### 1. 重启 IDE
保存配置文件后，重启你的 IDE 或 AI 客户端。

### 2. 测试连接
在 AI 对话中输入：
\`\`\`
请创建一个测试任务
\`\`\`

### 3. 查看统计
访问 [统计面板](https://agent.mark-chat.chat) 查看工具使用情况。

## 故障排除

### 连接问题
- 检查网络连接
- 确认 URL 地址正确
- 验证配置文件格式

### 配置无效
- 重启 IDE/客户端
- 检查 JSON 语法
- 查看错误日志

### 权限问题
- 确保配置文件可写
- 检查防火墙设置
- 验证 SSL 证书

## API 端点

### MCP 服务器
- **URL**: \`https://agent.mark-chat.chat/mcp\`
- **协议**: HTTP/HTTPS
- **版本**: MCP 1.0

### 统计 API
- **概览**: \`GET /api/statistics/overview\`
- **趋势**: \`GET /api/statistics/trend\`
- **客户端**: \`GET /api/statistics/clients/overview\`

## 支持的工具

### 当前可用
- **TodoWrite**: 任务管理工具

### 即将推出
- **FileManager**: 文件操作工具
- **ProjectAnalyzer**: 项目分析工具
- **CodeGenerator**: 代码生成工具

## 最佳实践

### 任务管理
- 使用明确的任务描述
- 设置合适的优先级
- 及时更新任务状态

### 性能优化
- 定期查看使用统计
- 避免创建过多任务
- 合理使用工具功能

## 更新日志

### v1.0.0 (2025-01-29)
- 初始发布
- TodoWrite 工具
- 基础统计功能
- 多 IDE 支持

## 联系支持

- **文档**: [https://agent.mark-chat.chat/docs](https://agent.mark-chat.chat/docs)
- **统计面板**: [https://agent.mark-chat.chat](https://agent.mark-chat.chat)
- **GitHub**: [项目地址]
- **Email**: support@mark-chat.chat

---

© 2025 Mark Agent. 基于 Model Context Protocol 构建的智能代理管理平台。
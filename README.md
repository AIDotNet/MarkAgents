# MarkAgent MCP Server

**MarkAgent.McpServer** 是一个基于 Model Context Protocol (MCP) 的智能任务管理服务器。它为 AI 助手提供了强大的 TODO 规划和任务管理能力，帮助 AI 更好地组织和跟踪复杂的多步骤任务。

## 🌟 功能特性

- **智能任务规划**: 自动将复杂任务分解为可管理的子任务
- **实时状态跟踪**: 支持 pending、in_progress、completed 三种任务状态
- **优先级管理**: 支持 high、medium、low 三个优先级等级
- **彩色控制台输出**: 根据优先级和状态显示不同颜色的任务信息
- **MCP 标准兼容**: 完全符合 Model Context Protocol 规范
- **易于集成**: 支持 VS Code、Visual Studio 等主流 IDE

## 🎯 使用场景

### 何时使用 TodoWrite 工具：

1. **复杂多步骤任务** - 需要 3 个或更多不同步骤的任务
2. **非平凡复杂任务** - 需要仔细规划或多个操作的任务
3. **用户明确请求** - 用户直接要求使用 todo 列表
4. **多个任务** - 用户提供任务列表（编号或逗号分隔）
5. **接收新指令后** - 立即将用户需求捕获为 todos
6. **开始工作时** - 在开始工作前标记为 in_progress
7. **完成任务后** - 标记为完成并添加新的后续任务

### 何时不使用：

1. 只有单个直接任务
2. 任务过于简单，跟踪无意义
3. 可在 3 个简单步骤内完成
4. 纯对话或信息性任务

## 📦 安装和配置

这个 MCP 服务器使用 C# MCP 服务器项目模板创建，展示了如何轻松创建 MCP 服务器并将其发布为 NuGet 包。

查看完整指南：[aka.ms/nuget/mcp/guide](https://aka.ms/nuget/mcp/guide)

**注意**: 此模板目前处于早期预览阶段。如有反馈，请参与[简短调查](http://aka.ms/dotnet-mcp-template-survey)。

## 🚀 发布前检查清单

- ✅ 使用下面的步骤在本地测试 MCP 服务器
- ✅ 更新 .csproj 文件中的包元数据，特别是 `<PackageId>`（已设置为 `MarkAgent.McpServer`）
- ✅ 更新 `.mcp/server.json` 以声明 MCP 服务器的输入
  - 查看[配置输入](https://aka.ms/nuget/mcp/guide/configuring-inputs)了解更多详情
- 使用 `dotnet pack` 打包项目

`bin/Release` 目录将包含可以[发布到 NuGet.org](https://learn.microsoft.com/nuget/nuget-org/publish-a-package) 的包文件 (.nupkg)。

## 🛠️ 本地开发

要从源代码本地测试此 MCP 服务器（无需使用构建的 MCP 服务器包），您可以配置 IDE 直接使用 `dotnet run` 运行项目。

```json
{
  "servers": {
    "MarkAgent.McpServer": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<项目目录路径>"
      ]
    }
  }
}
```

### 本地开发配置示例

```json
{
  "servers": {
    "MarkAgent.McpServer": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\code\\Agent\\MarkAgent.McpServer"
      ]
    }
  }
}
```

## 🧪 测试 MCP 服务器

配置完成后，您可以向支持 MCP 的 AI 助手请求任务管理功能。例如：

### 测试示例

1. **复杂任务规划**：
   ```
   "帮我实现一个用户注册系统，包括前端表单、后端验证、数据库存储和邮件验证功能"
   ```
   
2. **多步骤任务**：
   ```
   "重构这个项目的认证模块，确保运行测试并构建成功"
   ```

AI 助手将自动使用 `TodoWrite` 工具创建结构化的任务列表，并在执行过程中实时更新任务状态。

### 预期行为

- AI 助手会自动识别复杂任务
- 创建带有优先级的任务列表
- 在控制台中显示彩色的任务状态
- 实时跟踪任务进度

## 📦 发布到 NuGet.org

1. 运行 `dotnet pack -c Release` 创建 NuGet 包
2. 使用以下命令发布到 NuGet.org：
   ```bash
   dotnet nuget push bin/Release/*.nupkg --api-key <your-api-key> --source https://api.nuget.org/v3/index.json
   ```

### 当前包信息

- **包 ID**: `MarkAgent.McpServer`
- **版本**: `0.1.0-beta`
- **描述**: 给AI提供TODO规划的能力，增强AI的任务管理和规划能力
- **标签**: AI, MCP, server, stdio, Todo

## 🚀 从 NuGet.org 使用 MCP 服务器

MCP 服务器包发布到 NuGet.org 后，您可以在首选 IDE 中配置它。VS Code 和 Visual Studio 都使用 `dnx` 命令从 NuGet.org 下载和安装 MCP 服务器包。

### IDE 配置

- **VS Code**: 创建 `<工作区目录>/.vscode/mcp.json` 文件
- **Visual Studio**: 创建 `<解决方案目录>\.mcp.json` 文件

### 配置示例

对于 VS Code 和 Visual Studio，配置文件使用以下服务器定义：

```json
{
  "servers": {
    "MarkAgent.McpServer": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "MarkAgent.McpServer",
        "--version",
        "0.1.0-beta",
        "--yes"
      ]
    }
  }
}
```

### Claude Code 集成

如果您使用 Claude Code，可以通过以下方式配置：

```json
{
  "servers": {
    "mark-agent": {
      "type": "stdio", 
      "command": "dnx",
      "args": [
        "MarkAgent.McpServer",
        "--version", 
        "0.1.0-beta",
        "--yes"
      ]
    }
  }
}
```

## 📚 更多信息

### 技术架构

本项目基于以下技术栈构建：

- **.NET 8.0**: 现代化的 .NET 运行时
- **ModelContextProtocol SDK**: [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) C# SDK
- **Microsoft.Extensions.Hosting**: 用于服务托管和依赖注入
- **System.Text.Json**: 高性能 JSON 序列化

### MCP 相关资源

了解更多关于 Model Context Protocol 的信息：

- [官方文档](https://modelcontextprotocol.io/)
- [协议规范](https://spec.modelcontextprotocol.io/)
- [GitHub 组织](https://github.com/modelcontextprotocol)

### IDE 集成文档

有关配置和使用 MCP 服务器的更多信息，请参阅 VS Code 或 Visual Studio 文档：

- [在 VS Code 中使用 MCP 服务器 (预览)](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [在 Visual Studio 中使用 MCP 服务器 (预览)](https://learn.microsoft.com/visualstudio/ide/mcp-servers)

### 开源贡献

欢迎为本项目贡献代码！请查看我们的 [GitHub 仓库](https://github.com/AIDotNet/MarkAgent.McpServer) 了解更多信息。

### 许可证

本项目采用开源许可证发布。详细信息请查看项目仓库中的 LICENSE 文件。

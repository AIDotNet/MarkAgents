# MarkAgents

<div align="center">

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![React](https://img.shields.io/badge/React-19.1.0-blue.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-blue.svg)
![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)
![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)

**一个基于 MCP (Model Context Protocol) 的智能代理平台**  
**An intelligent agent platform based on MCP (Model Context Protocol)**

[English](#english) | [中文](#中文)

</div>

---

## 中文

### 📖 项目简介

MarkAgents 是一个现代化的智能代理平台，基于 Model Context Protocol (MCP) 构建。该平台提供了强大的任务管理、统计分析和客户端连接跟踪功能，专为开发者和团队协作而设计。

### ✨ 核心特性

- 🚀 **MCP 服务器实现** - 完整的 Model Context Protocol 服务器支持
- 📊 **实时统计分析** - 工具使用统计、客户端连接跟踪
- ✅ **智能任务管理** - 基于优先级的 TODO 系统，支持控制台彩色输出
- 🔄 **后台数据处理** - 基于 Channel 的异步事件处理系统
- 🌐 **现代化 Web 界面** - React + TypeScript 构建的响应式管理界面
- 🔐 **安全认证** - JWT 令牌认证和用户管理
- 📦 **容器化部署** - 完整的 Docker 支持
- 🎨 **主题切换** - 支持深色/浅色模式

### 🛠️ 技术栈

#### 后端
- **.NET 9.0** - 最新的 .NET 框架
- **ASP.NET Core** - Web API 框架
- **Entity Framework Core** - ORM 数据访问
- **SQLite** - 轻量级数据库
- **ModelContextProtocol** - MCP 协议实现
- **JWT Bearer** - 身份认证
- **BCrypt.Net** - 密码加密

#### 前端
- **React 19.1.0** - 用户界面库
- **TypeScript** - 类型安全的 JavaScript
- **Vite** - 快速构建工具
- **Tailwind CSS** - 实用优先的 CSS 框架
- **Radix UI** - 无障碍 UI 组件
- **React Router** - 客户端路由
- **Recharts** - 数据可视化

#### 开发工具
- **Docker** - 容器化
- **ESLint** - 代码质量检查
- **Scalar** - API 文档生成

### 🚀 快速开始

#### 前置要求

- **.NET 9.0 SDK**
- **Node.js 18+**
- **Docker** (可选)

#### 本地开发

1. **克隆仓库**
```bash
git clone https://github.com/yourusername/MarkAgents.git
cd MarkAgents
```

2. **启动后端服务**
```bash
cd src/MarkAgent.Host
dotnet restore
dotnet run
```

3. **启动前端服务**
```bash
cd web
npm install
npm run dev
```

4. **访问应用**
- 前端界面：http://localhost:5173
- API 服务：http://localhost:5000
- MCP 端点：http://localhost:5000/mcp

#### Docker 部署

```bash
# 构建并运行
docker-compose up -d

# 访问应用
curl http://localhost:18183
```

#### 快速脚本

**Windows:**
```batch
# 构建并运行
.\build-and-run.bat

# 仅启动
.\start.bat
```

**Linux/macOS:**
```bash
# 构建并运行
./build-and-run.sh

# 仅启动
./start.sh
```

### 📁 项目结构

```
MarkAgents/
├── src/MarkAgent.Host/          # .NET 后端服务
│   ├── Apis/                   # API 控制器
│   ├── Domain/                 # 领域模型和服务
│   ├── Infrastructure/         # 基础设施层
│   ├── Tools/                  # MCP 工具实现
│   └── Prompts/               # AI 提示词模板
├── web/                        # React 前端应用
│   ├── src/components/         # UI 组件
│   ├── src/pages/             # 页面组件
│   └── src/lib/               # 工具函数
├── docker-compose.yaml         # Docker 编排
└── MarkAgent.sln              # .NET 解决方案
```

### 🔧 开发指南

#### 后端开发

```bash
# 数据库迁移
dotnet ef migrations add <MigrationName>
dotnet ef database update

# 运行测试
dotnet test

# 发布构建
dotnet publish -c Release
```

#### 前端开发

```bash
# 安装依赖
npm install

# 开发模式
npm run dev

# 类型检查
npm run lint

# 生产构建
npm run build
```

### 📊 核心功能

#### MCP 工具系统
- **TodoWrite** - 智能任务管理工具
- **统计跟踪** - 自动记录工具使用情况
- **客户端管理** - 连接状态和会话跟踪

#### 数据分析
- **实时统计** - 工具使用频率分析
- **客户端洞察** - 连接模式和用户行为
- **历史趋势** - 长期数据趋势分析

#### 安全特性
- **JWT 认证** - 安全的用户身份验证
- **CORS 支持** - 跨域资源共享配置
- **数据加密** - 敏感信息保护

### 🤝 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

### 📄 许可证

本项目采用 MIT 许可证。详情请查看 [LICENSE](LICENSE) 文件。

### 🙏 致谢

感谢所有贡献者和开源社区的支持！

---

## English

### 📖 Project Description

MarkAgents is a modern intelligent agent platform built on the Model Context Protocol (MCP). This platform provides powerful task management, statistical analysis, and client connection tracking capabilities, designed specifically for developers and team collaboration.

### ✨ Key Features

- 🚀 **MCP Server Implementation** - Complete Model Context Protocol server support
- 📊 **Real-time Statistics** - Tool usage analytics and client connection tracking
- ✅ **Smart Task Management** - Priority-based TODO system with colorful console output
- 🔄 **Background Data Processing** - Channel-based asynchronous event processing system
- 🌐 **Modern Web Interface** - Responsive management interface built with React + TypeScript
- 🔐 **Secure Authentication** - JWT token authentication and user management
- 📦 **Containerized Deployment** - Complete Docker support
- 🎨 **Theme Switching** - Support for dark/light mode

### 🛠️ Technology Stack

#### Backend
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM data access
- **SQLite** - Lightweight database
- **ModelContextProtocol** - MCP protocol implementation
- **JWT Bearer** - Authentication
- **BCrypt.Net** - Password encryption

#### Frontend
- **React 19.1.0** - User interface library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool
- **Tailwind CSS** - Utility-first CSS framework
- **Radix UI** - Accessible UI components
- **React Router** - Client-side routing
- **Recharts** - Data visualization

#### Development Tools
- **Docker** - Containerization
- **ESLint** - Code quality checking
- **Scalar** - API documentation generation

### 🚀 Quick Start

#### Prerequisites

- **.NET 9.0 SDK**
- **Node.js 18+**
- **Docker** (optional)

#### Local Development

1. **Clone Repository**
```bash
git clone https://github.com/yourusername/MarkAgents.git
cd MarkAgents
```

2. **Start Backend Service**
```bash
cd src/MarkAgent.Host
dotnet restore
dotnet run
```

3. **Start Frontend Service**
```bash
cd web
npm install
npm run dev
```

4. **Access Application**
- Frontend: http://localhost:5173
- API Service: http://localhost:5000
- MCP Endpoint: http://localhost:5000/mcp

#### Docker Deployment

```bash
# Build and run
docker-compose up -d

# Access application
curl http://localhost:18183
```

#### Quick Scripts

**Windows:**
```batch
# Build and run
.\build-and-run.bat

# Start only
.\start.bat
```

**Linux/macOS:**
```bash
# Build and run
./build-and-run.sh

# Start only
./start.sh
```

### 📁 Project Structure

```
MarkAgents/
├── src/MarkAgent.Host/          # .NET backend service
│   ├── Apis/                   # API controllers
│   ├── Domain/                 # Domain models and services
│   ├── Infrastructure/         # Infrastructure layer
│   ├── Tools/                  # MCP tool implementations
│   └── Prompts/               # AI prompt templates
├── web/                        # React frontend application
│   ├── src/components/         # UI components
│   ├── src/pages/             # Page components
│   └── src/lib/               # Utility functions
├── docker-compose.yaml         # Docker orchestration
└── MarkAgent.sln              # .NET solution
```

### 🔧 Development Guide

#### Backend Development

```bash
# Database migration
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Run tests
dotnet test

# Release build
dotnet publish -c Release
```

#### Frontend Development

```bash
# Install dependencies
npm install

# Development mode
npm run dev

# Type checking
npm run lint

# Production build
npm run build
```

### 📊 Core Features

#### MCP Tool System
- **TodoWrite** - Smart task management tool
- **Statistics Tracking** - Automatic tool usage recording
- **Client Management** - Connection status and session tracking

#### Data Analytics
- **Real-time Statistics** - Tool usage frequency analysis
- **Client Insights** - Connection patterns and user behavior
- **Historical Trends** - Long-term data trend analysis

#### Security Features
- **JWT Authentication** - Secure user identity verification
- **CORS Support** - Cross-origin resource sharing configuration
- **Data Encryption** - Sensitive information protection

### 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Create a Pull Request

### 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

### 🙏 Acknowledgments

Thanks to all contributors and the open source community for their support!

---

<div align="center">

### ⭐ Star History

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/MarkAgents&type=Date)](https://star-history.com/#yourusername/MarkAgents&Date)

**如果这个项目对您有帮助，请考虑给它一个 ⭐️**  
**If this project helps you, please consider giving it a ⭐️**

---

![Visitor Count](https://visitor-badge.laobi.icu/badge?page_id=yourusername.MarkAgents)
![Last Commit](https://img.shields.io/github/last-commit/yourusername/MarkAgents)
![Issues](https://img.shields.io/github/issues/yourusername/MarkAgents)
![Pull Requests](https://img.shields.io/github/issues-pr/yourusername/MarkAgents)

</div>

# MarkAgents

<div align="center">

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![React](https://img.shields.io/badge/React-19.1.0-blue.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-blue.svg)
![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)
![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)

**An intelligent agent platform based on MCP (Model Context Protocol)**

[中文文档](README_ZH.md) | English

</div>

---

## 📖 Project Description

MarkAgents is a modern intelligent agent platform built on the Model Context Protocol (MCP). This platform provides powerful task management, statistical analysis, and client connection tracking capabilities, designed specifically for developers and team collaboration.

## ✨ Key Features

- 🚀 **MCP Server Implementation** - Complete Model Context Protocol server support
- 📊 **Real-time Statistics** - Tool usage analytics and client connection tracking
- ✅ **Smart Task Management** - Priority-based TODO system with colorful console output
- 🔄 **Background Data Processing** - Channel-based asynchronous event processing system
- 🌐 **Modern Web Interface** - Responsive management interface built with React + TypeScript
- 🔐 **Secure Authentication** - JWT token authentication and user management
- 📦 **Containerized Deployment** - Complete Docker support
- 🎨 **Theme Switching** - Support for dark/light mode

## 🛠️ Technology Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM data access
- **SQLite** - Lightweight database
- **ModelContextProtocol** - MCP protocol implementation
- **JWT Bearer** - Authentication
- **BCrypt.Net** - Password encryption

### Frontend
- **React 19.1.0** - User interface library
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool
- **Tailwind CSS** - Utility-first CSS framework
- **Radix UI** - Accessible UI components
- **React Router** - Client-side routing
- **Recharts** - Data visualization

### Development Tools
- **Docker** - Containerization
- **ESLint** - Code quality checking
- **Scalar** - API documentation generation

## 🚀 Quick Start

### Prerequisites

- **.NET 9.0 SDK**
- **Node.js 18+**
- **Docker** (optional)

### Local Development

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

### Docker Deployment

```bash
# Build and run
docker-compose up -d

# Access application
curl http://localhost:18183
```

### Quick Scripts

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

## 📁 Project Structure

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

## 🔧 Development Guide

### Backend Development

```bash
# Database migration
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Run tests
dotnet test

# Release build
dotnet publish -c Release
```

### Frontend Development

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

## 📊 Core Features

### MCP Tool System
- **TodoWrite** - Smart task management tool
- **Statistics Tracking** - Automatic tool usage recording
- **Client Management** - Connection status and session tracking

### Data Analytics
- **Real-time Statistics** - Tool usage frequency analysis
- **Client Insights** - Connection patterns and user behavior
- **Historical Trends** - Long-term data trend analysis

### Security Features
- **JWT Authentication** - Secure user identity verification
- **CORS Support** - Cross-origin resource sharing configuration
- **Data Encryption** - Sensitive information protection

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Thanks to all contributors and the open source community for their support!

---

<div align="center">

### ⭐ Star History

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/MarkAgents&type=Date)](https://star-history.com/#yourusername/MarkAgents&Date)

**If this project helps you, please consider giving it a ⭐️**

---

![Visitor Count](https://visitor-badge.laobi.icu/badge?page_id=yourusername.MarkAgents)
![Last Commit](https://img.shields.io/github/last-commit/yourusername/MarkAgents)
![Issues](https://img.shields.io/github/issues/yourusername/MarkAgents)
![Pull Requests](https://img.shields.io/github/issues-pr/yourusername/MarkAgents)

</div>

# MarkAgent - Advanced Todo Management with MCP Integration

A comprehensive todo management system built with .NET 9, designed around Domain-Driven Design (DDD) principles and integrated with the official Model Context Protocol (MCP) for AI agent interactions.

## 🌟 Key Features

### 🏗️ Architecture & Design
- **DDD Architecture**: Clean separation of concerns with Domain, Application, Infrastructure, and API layers
- **Multi-API Key Management**: Users can create multiple API keys with granular MCP service permissions
- **Role-Based Access**: Admin and User roles with appropriate permissions
- **Official MCP Integration**: Uses the latest official C# SDK for Model Context Protocol

### 🔐 User Management
- **Email Registration/Login**: Secure authentication with email verification
- **Password Reset**: Secure password reset via email with temporary tokens
- **JWT Authentication**: Stateless authentication with configurable expiration
- **Default Admin Account**: Automatically created admin account for system management

### 🔑 Advanced API Key System
- **Multiple Keys per User**: Create and manage multiple API keys
- **Service-Specific Permissions**: Each key can enable/disable specific MCP services
- **Key Metadata**: Customizable names, descriptions, expiration dates
- **Security Features**: IP restrictions, rate limiting, usage tracking

### 🛠️ MCP Service Management
- **Pluggable Architecture**: Easy to add new MCP services
- **Admin Service Management**: Admins can add/remove/configure MCP services
- **User Service Selection**: Users choose which services to enable per API key
- **Usage Analytics**: Track service usage per key and user

### ✅ Todo Management
- **Session-Based Organization**: Todos organized by conversation sessions
- **IAsyncDisposable Support**: Automatic persistence when sessions end
- **Real-time Updates**: SSE for live todo status changes
- **Rich Metadata**: Priorities, due dates, completion tracking

### 📊 Statistics & Analytics
- **User Statistics**: Todo creation/completion rates per user
- **System Statistics**: Overall system usage metrics
- **API Key Analytics**: Usage patterns per API key
- **Extensible Design**: Easy to add new statistical metrics

## 🏃‍♂️ Quick Start

### Prerequisites
- .NET 9.0 SDK
- SQLite (embedded)

### 1. Clone and Build
```bash
git clone <repository-url>
cd MarkAgent
dotnet build
```

### 2. Configure Email Settings
Edit `src/MarkAgent.Api/appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "MarkAgent"
  }
}
```

### 3. Run the API Server
```bash
cd src/MarkAgent.Api
dotnet run
```

### 4. Run the MCP Server
```bash
cd src/MarkAgent.McpServer
dotnet run
```

## 🔐 Default Admin Account

The system automatically creates a default admin account:
- **Email**: `admin@markagent.com`
- **Password**: `Admin123!`
- **API Key**: `sk-admin-default-key-12345`

⚠️ **Change these credentials in production!**

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token

### API Key Management
- `GET /api/api-keys` - List user's API keys
- `POST /api/api-keys` - Create new API key
- `PUT /api/api-keys/{id}` - Update API key
- `DELETE /api/api-keys/{id}` - Delete API key
- `PUT /api/api-keys/{id}/mcp-services` - Configure MCP services for key

### Todo Management
- `GET /api/todos` - List user todos
- `POST /api/todos` - Create new todo
- `PUT /api/todos/{id}` - Update todo
- `DELETE /api/todos/{id}` - Delete todo
- `PATCH /api/todos/{id}/status` - Update todo status

### Statistics
- `GET /api/statistics/user` - User statistics
- `GET /api/statistics/system` - System statistics (admin only)

### MCP Services (Admin Only)
- `GET /api/mcp-services` - List available MCP services
- `POST /api/mcp-services/admin` - Create new MCP service
- `PUT /api/mcp-services/admin/{id}` - Update MCP service

### Real-time Updates
- `GET /api/sse/todos` - Server-Sent Events for todo updates

## 🤖 MCP Integration

### Using the MCP Server

The MCP server provides a `manage_todo` tool that supports:

1. **Create Session**: Start a new conversation session
2. **Create Todo**: Add todos to a session
3. **Update Todo**: Modify existing todos
4. **List Todos**: Retrieve todos (all or by session)
5. **Update Status**: Change todo completion status
6. **End Session**: Close and persist session data

### Example MCP Tool Usage

```json
{
  "action": "create_session",
  "user_key": "sk-your-api-key",
  "session_name": "Code Review Session"
}
```

```json
{
  "action": "create_todo",
  "user_key": "sk-your-api-key",
  "session_id": "session-uuid",
  "todo_data": "{\"title\":\"Review authentication\",\"description\":\"Check JWT implementation\"}"
}
```

### Integration with AI Agents

The MCP server can be used with any MCP-compatible AI agent or client:

```bash
# Using with Claude Desktop, VS Code, or other MCP clients
npx @modelcontextprotocol/inspector build/MarkAgent.McpServer.dll
```

## 🏗️ Architecture Overview

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   MCP Clients   │    │    Web API       │    │   Admin Panel   │
│  (AI Agents)    │    │   (REST API)     │    │   (Future)      │
└─────────┬───────┘    └─────────┬────────┘    └─────────────────┘
          │                      │                      
          │              ┌───────▼────────┐             
          │              │  MarkAgent.Api │             
          │              │   (Mini APIs)  │             
          │              └───────┬────────┘             
          │                      │                      
          │              ┌───────▼────────┐             
          │              │ Application    │             
          │              │    Layer       │             
          │              └───────┬────────┘             
          │                      │                      
          │              ┌───────▼────────┐             
          │              │ Infrastructure │             
          │              │    Layer       │             
          │              └───────┬────────┘             
          │                      │                      
          │              ┌───────▼────────┐             
┌─────────▼───────┐      │    Domain      │             
│ MarkAgent.MCP   │      │     Layer      │             
│    Server       │      └───────┬────────┘             
│ (MCP Protocol)  │              │                      
└─────────────────┘      ┌───────▼────────┐             
                         │    SQLite      │             
                         │   Database     │             
                         └────────────────┘             
```

## 🔧 Configuration

### JWT Settings
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "MarkAgent.Api",
    "Audience": "MarkAgent.Client",
    "ExpirationMinutes": 1440
  }
}
```

### Database
The system uses SQLite by default. Connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=markagent.db"
  }
}
```

## 🚀 Development

### Adding New MCP Services

1. Create a new tool class implementing the MCP interface
2. Register it in the MCP server configuration
3. Add service metadata to the database
4. Users can then enable/disable it per API key

### Extending Statistics

Add new metrics by:
1. Extending the `UserStatistics` entity
2. Creating corresponding DTOs
3. Updating the statistics service
4. Adding new API endpoints

## 📝 Example Usage Scenarios

### Scenario 1: AI-Powered Code Review
1. AI agent creates a session: "Code Review for PR #123"
2. Agent analyzes code and creates todos for each issue found
3. Developer reviews todos in real-time via SSE
4. Agent updates todo status as issues are resolved
5. Session automatically persists when complete

### Scenario 2: Team Task Management
1. Multiple team members have different API keys
2. Each key enables different MCP services based on role
3. Project manager key: full access to all services
4. Developer key: limited to todo and session management
5. Analytics track usage patterns across the team

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow DDD principles and maintain clean architecture
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Built with the official [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- Inspired by Domain-Driven Design principles
- Uses modern .NET 9 features and best practices

```
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.
```

## Development Commands

### Frontend (web/)
- `npm run dev`: Start development server
- `npm run build`: Build production assets
- `npm run lint`: Run ESLint
- `npm run preview`: Preview production build

### Backend (src/MarkAgent.Host/)
- `dotnet run`: Start API server
- `dotnet build`: Build project
- `dotnet ef migrations add <Name>`: Create database migration
- `dotnet ef database update`: Apply migrations

## Architecture Overview

This repository contains a two-component application:

1. **Backend API** (.NET 9)
   - ASP.NET Core web application with SQLite database
   - MCP server implementation at `/mcp` endpoint
   - Statistics tracking system with background processing
   - RESTful API controllers with CORS support
   - Entity Framework Core for data access
   - JWT-based authentication

2. **Frontend** (React + TypeScript)
   - Vite development environment
   - React Router for navigation
   - Protected routes with authentication
   - Admin dashboard with statistics views
   - Theme switching capability

Key domain concepts include client connection tracking, tool usage statistics, and a priority-based TODO system with console output integration.
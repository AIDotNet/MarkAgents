@echo off
echo 🚀 Starting MarkAgent System...

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET is not installed. Please install .NET 9.0 SDK first.
    echo    Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)

REM Build the solution
echo 🔨 Building solution...
dotnet build --configuration Release >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Build failed. Please check for compilation errors.
    pause
    exit /b 1
)
echo ✅ Build successful

echo.
echo 📋 Default Admin Account:
echo    Email: admin@markagent.com
echo    Password: Admin123!
echo    API Key: sk-admin-default-key-12345
echo.
echo 🔗 URLs:
echo    API Server: http://localhost:5000
echo    Swagger UI: http://localhost:5000/swagger
echo    Health Check: http://localhost:5000/health
echo.

REM Start API Server
echo 🌐 Starting API Server on port 5000...
start "MarkAgent API" /D "src\MarkAgent.Api" dotnet run --configuration Release --urls="http://localhost:5000"

REM Wait a moment for API server to start
timeout /t 3 /nobreak >nul

REM Start MCP Server
echo 🤖 Starting MCP Server...
start "MarkAgent MCP" /D "src\MarkAgent.McpServer" dotnet run --configuration Release

echo.
echo 🎉 MarkAgent system is ready!
echo    Two console windows should have opened for the servers.
echo    Press any key to exit this script (servers will continue running).
echo.

pause >nul
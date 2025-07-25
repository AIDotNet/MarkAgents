#!/bin/bash

echo "🚀 Starting MarkAgent System..."

# Function to check if .NET 9 is installed
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        echo "❌ .NET is not installed. Please install .NET 9.0 SDK first."
        echo "   Download from: https://dotnet.microsoft.com/download/dotnet/9.0"
        exit 1
    fi
    
    # Check .NET version
    DOTNET_VERSION=$(dotnet --version)
    if [[ ! $DOTNET_VERSION == 9.* ]]; then
        echo "⚠️  Warning: .NET 9.0 is recommended. Current version: $DOTNET_VERSION"
    fi
}

# Function to build the solution
build_solution() {
    echo "🔨 Building solution..."
    if ! dotnet build --configuration Release > /dev/null 2>&1; then
        echo "❌ Build failed. Please check for compilation errors."
        exit 1
    fi
    echo "✅ Build successful"
}

# Function to start API server
start_api_server() {
    echo "🌐 Starting API Server on port 5000..."
    cd src/MarkAgent.Api
    dotnet run --configuration Release --urls="http://localhost:5000" &
    API_PID=$!
    cd ../..
    echo "✅ API Server started (PID: $API_PID)"
}

# Function to start MCP server
start_mcp_server() {
    echo "🤖 Starting MCP Server..."
    cd src/MarkAgent.McpServer
    dotnet run --configuration Release &
    MCP_PID=$!
    cd ../..
    echo "✅ MCP Server started (PID: $MCP_PID)"
}

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "🛑 Shutting down servers..."
    if [ ! -z "$API_PID" ]; then
        kill $API_PID 2>/dev/null
        echo "✅ API Server stopped"
    fi
    if [ ! -z "$MCP_PID" ]; then
        kill $MCP_PID 2>/dev/null
        echo "✅ MCP Server stopped"
    fi
    exit 0
}

# Set up trap for cleanup
trap cleanup SIGINT SIGTERM

# Main execution
check_dotnet
build_solution

echo ""
echo "📋 Default Admin Account:"
echo "   Email: admin@markagent.com"
echo "   Password: Admin123!"
echo "   API Key: sk-admin-default-key-12345"
echo ""
echo "🔗 URLs:"
echo "   API Server: http://localhost:5000"
echo "   Swagger UI: http://localhost:5000/swagger"
echo "   Health Check: http://localhost:5000/health"
echo ""

start_api_server
sleep 2
start_mcp_server

echo ""
echo "🎉 MarkAgent system is ready!"
echo "   Press Ctrl+C to stop all servers"
echo ""

# Wait for interrupt
wait
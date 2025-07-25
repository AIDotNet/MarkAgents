using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Sdk.Server;
using MarkAgent.McpServer;
using MarkAgent.Infrastructure.Extensions;
using MarkAgent.Application.Extensions;
using MarkAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Add database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=markagent.db"));

// Add custom services
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();

// Configure MCP Server with official SDK
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<McpTodoServer>();

await builder.Build().RunAsync();
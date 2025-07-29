using MarkAgent.Host.Tools;
using MarkAgent.Host.Infrastructure.Data;
using MarkAgent.Host.Infrastructure.Services;
using MarkAgent.Host.Infrastructure.Repositories;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Domain.Services;
using MarkAgent.Host.Domain.Events;
using MarkAgent.Host.Apis;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器和API服务
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 配置数据库
builder.Services.AddDbContext<StatisticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=statistics.db"));

// 注册仓储
builder.Services.AddScoped<IToolUsageRepository, ToolUsageRepository>();
builder.Services.AddScoped<IDailyToolStatisticsRepository, DailyToolStatisticsRepository>();
builder.Services.AddScoped<IClientConnectionRepository, ClientConnectionRepository>();
builder.Services.AddScoped<IDailyClientStatisticsRepository, DailyClientStatisticsRepository>();

// 注册服务
builder.Services.AddScoped<IToolStatisticsService, ToolStatisticsService>();

// 注册Channel服务 (单例)
builder.Services.AddSingleton<StatisticsChannelService>();
builder.Services.AddSingleton<IStatisticsChannelService>(provider =>
    provider.GetRequiredService<StatisticsChannelService>());

// 注册后台处理服务
builder.Services.AddHostedService<StatisticsProcessorService>();

// 添加CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services
    .AddMcpServer((options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "MarkAgent",
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Title = "MarkAgent MCP Server",
        };
    }))
    .WithHttpTransport(options =>
    {
        options.RunSessionHandler += async (context, serverOptions, arg3) =>
        {
            try
            {
                // 获取Channel服务
                var channelService = context.RequestServices.GetRequiredService<IStatisticsChannelService>();

                // 获取客户端信息
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers.UserAgent.ToString();

                // 获取请求客户端信息
                var clientName = userAgent;
                var clientVersion = serverOptions?.ClientInfo?.Version ?? "0.0.0";
                var clientTitle = userAgent;

                // 生成会话ID
                var sessionId = serverOptions.SessionId;

                // 只有在有UserAgent的情况下才记录客户端统计
                if (!string.IsNullOrEmpty(userAgent))
                {
                    // 创建客户端连接事件
                    var connectionEvent = new ClientConnectionEvent
                    {
                        ClientName = clientName,
                        ClientVersion = clientVersion,
                        ClientTitle = clientTitle,
                        SessionId = sessionId,
                        ConnectionTime = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        UserAgent = userAgent,
                        ProtocolVersion = "MarkAgent/1.0",
                        ClientCapabilities = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            serverOptions.ClientInfo?.Name,
                            serverOptions.ClientInfo?.Version,
                            serverOptions.ClientInfo?.Title
                        })
                    };

                    // 写入Channel
                    await channelService.WriteClientConnectionEventAsync(connectionEvent);
                }

                // 将会话ID存储到上下文中，以便在工具调用时使用
                context.Items["SessionId"] = sessionId;

                Console.WriteLine($"Client connected: {clientName} v{clientVersion} (Session: {sessionId[..8]}...)");

                await serverOptions.RunAsync();
            }
            catch (Exception ex)
            {
                // 记录错误但不影响连接
                Console.WriteLine($"❌ Error recording client connection: {ex.Message}");
            }
        };
    })
    .WithTools<AgentTools>();

var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "MarkAgent API";
        options.DarkMode = true;
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// 配置静态文件服务
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 缓存静态文件
        var headers = ctx.Context.Response.Headers;
        var path = ctx.File.Name;
        
        // 为静态资源设置缓存头
        if (path.EndsWith(".js") || path.EndsWith(".css") || 
            path.EndsWith(".png") || path.EndsWith(".jpg") || 
            path.EndsWith(".jpeg") || path.EndsWith(".gif") || 
            path.EndsWith(".ico") || path.EndsWith(".svg"))
        {
            headers.Append("Cache-Control", "public,max-age=31536000");
            headers.Append("Expires", DateTime.UtcNow.AddYears(1).ToString("R"));
        }
    }
});

app.MapControllers();

app.MapMcp("/mcp");

// 映射统计Channel API
app.MapStatisticsChannelEndpoints();

// SPA路由处理 - 所有未匹配的路由都返回index.html
app.MapFallbackToFile("index.html");

// 初始化数据库
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StatisticsDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        // 记录日志但不阻塞应用启动
        Console.WriteLine($"Database initialization warning: {ex.Message}");
    }
}

await app.RunAsync();
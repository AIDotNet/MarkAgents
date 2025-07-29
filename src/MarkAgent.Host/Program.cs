using MarkAgent.Host.Infrastructure.Extensions;
using MarkAgent.Host.Apis;
using MarkAgent.Host.Tools;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 添加基础设施服务
builder.Services.AddInfrastructure(builder.Configuration);

// 添加控制器和API服务
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<AgentTools>();

var app = builder.Build();

// 初始化数据库
await app.Services.InitializeDatabaseAsync();

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

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// 注册Minimal APIs
app.MapAllApis();

app.MapMcp("/mcp");

await app.RunAsync();
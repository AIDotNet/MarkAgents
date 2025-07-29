using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Text;
using MarkAgent.Host.Domain.Repositories;
using MarkAgent.Host.Infrastructure.Data;
using MarkAgent.Host.Infrastructure.Repositories;
using MarkAgent.Host.Services;

namespace MarkAgent.Host.Infrastructure.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加基础设施服务
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加数据库上下文
        services.AddDbContext<MarkAgentDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);

            // 开发环境启用敏感数据日志
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // 注册仓储
        services.AddScoped(typeof(IRepository<,>), typeof(BaseRepository<,>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserKeyRepository, UserKeyRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IUserStatisticsRepository, UserStatisticsRepository>();
        services.AddScoped<ISystemStatisticsRepository, SystemStatisticsRepository>();
        services.AddScoped<IMcpToolConfigRepository, McpToolConfigRepository>();
        services.AddScoped<IMcpToolUsageRepository, McpToolUsageRepository>();
        services.AddScoped<ISystemMcpToolRepository, SystemMcpToolRepository>();
        services.AddScoped<IEmailVerificationCodeRepository, EmailVerificationCodeRepository>();
        services.AddScoped<IOAuthConfigRepository, OAuthConfigRepository>();
        services.AddScoped<IUserOAuthBindingRepository, UserOAuthBindingRepository>();

        // 配置邮箱服务选项
        services.Configure<EmailOptions>(configuration.GetSection("Email"));

        // 配置OAuth选项
        services.Configure<OAuthOptions>(configuration.GetSection("OAuth"));

        // 注册业务服务
        services.AddScoped<JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserKeyService, UserKeyService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IMcpService, McpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IOAuthService, OAuthService>();
        services.AddScoped<McpToolInitializationService>();

        // 添加JWT认证
        AddJwtAuthentication(services, configuration);

        // 添加内存缓存
        services.AddMemoryCache();

        // 添加HttpClient
        services.AddHttpClient();

        return services;
    }

    /// <summary>
    /// 添加JWT认证配置
    /// </summary>
    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static async Task<IServiceProvider> InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MarkAgentDbContext>();

        // 确保数据库创建
        await context.Database.EnsureCreatedAsync();

        // 应用待处理的迁移
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }

        // 初始化OAuth配置数据
        var oauthOptions = scope.ServiceProvider.GetRequiredService<IOptions<OAuthOptions>>();
        await OAuthConfigSeeder.SeedAsync(context, oauthOptions);

        return serviceProvider;
    }
}
using MarkAgent.Host.Domain.Entities;
using MarkAgent.Host.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarkAgent.Host.Infrastructure.Data;

/// <summary>
/// OAuth配置数据同步器
/// </summary>
public static class OAuthConfigSeeder
{
    /// <summary>
    /// 从配置文件同步OAuth配置到数据库
    /// </summary>
    public static async Task SeedAsync(MarkAgentDbContext context, IOptions<OAuthOptions> oauthOptions)
    {
        var configuredProviders = oauthOptions.Value.Providers;
        
        // 获取数据库中现有的配置
        var existingConfigs = await context.OAuthConfigs.ToListAsync();
        
        foreach (var providerConfig in configuredProviders)
        {
            var provider = providerConfig.Key;
            var config = providerConfig.Value;
            
            // 查找现有配置
            var existingConfig = existingConfigs.FirstOrDefault(c => c.Provider == provider);
            
            if (existingConfig == null)
            {
                // 创建新配置
                var newConfig = new OAuthConfig
                {
                    Provider = provider,
                    ClientId = config.ClientId,
                    ClientSecret = config.ClientSecret,
                    AuthorizeUrl = config.AuthorizeUrl,
                    TokenUrl = config.TokenUrl,
                    UserInfoUrl = config.UserInfoUrl,
                    RedirectUri = config.RedirectUri,
                    Scope = config.Scope,
                    IsEnabled = config.IsEnabled,
                    Sort = config.Sort,
                    Description = config.Description
                };
                
                await context.OAuthConfigs.AddAsync(newConfig);
            }
            else
            {
                // 更新现有配置（只更新关键配置，保留数据库中的启用状态等运行时设置）
                existingConfig.ClientId = config.ClientId;
                existingConfig.ClientSecret = config.ClientSecret;
                existingConfig.AuthorizeUrl = config.AuthorizeUrl;
                existingConfig.TokenUrl = config.TokenUrl;
                existingConfig.UserInfoUrl = config.UserInfoUrl;
                existingConfig.RedirectUri = config.RedirectUri;
                existingConfig.Scope = config.Scope;
                existingConfig.Sort = config.Sort;
                existingConfig.Description = config.Description;
                
                // 如果配置文件中明确禁用，则更新启用状态
                if (!config.IsEnabled)
                {
                    existingConfig.IsEnabled = false;
                }
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    /// <summary>
    /// 获取默认OAuth配置模板
    /// </summary>
    public static Dictionary<string, OAuthProviderConfig> GetDefaultConfigs()
    {
        return new Dictionary<string, OAuthProviderConfig>
        {
            [OAuthProviders.GitHub] = new OAuthProviderConfig
            {
                AuthorizeUrl = "https://github.com/login/oauth/authorize",
                TokenUrl = "https://github.com/login/oauth/access_token",
                UserInfoUrl = "https://api.github.com/user",
                Scope = "user:email",
                Description = "GitHub第三方登录",
                Sort = 1
            },
            
            [OAuthProviders.Gitee] = new OAuthProviderConfig
            {
                AuthorizeUrl = "https://gitee.com/oauth/authorize",
                TokenUrl = "https://gitee.com/oauth/token",
                UserInfoUrl = "https://gitee.com/api/v5/user",
                Scope = "user_info emails",
                Description = "Gitee第三方登录",
                Sort = 2
            },
            
            [OAuthProviders.Google] = new OAuthProviderConfig
            {
                AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth",
                TokenUrl = "https://oauth2.googleapis.com/token",
                UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo",
                Scope = "openid email profile",
                Description = "Google第三方登录",
                Sort = 3
            }
        };
    }
} 
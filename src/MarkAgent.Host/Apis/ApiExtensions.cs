namespace MarkAgent.Host.Apis;

/// <summary>
/// API扩展方法
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    /// 注册所有Minimal APIs
    /// </summary>
    public static IEndpointRouteBuilder MapAllApis(this IEndpointRouteBuilder endpoints)
    {
        // 注册认证API
        endpoints.MapAuthApi();
        
        // 注册用户密钥API
        endpoints.MapUserKeyApi();
        
        // 注册Todo API
        endpoints.MapTodoApi();
        
        // 注册邮箱API
        endpoints.MapEmailApi();
        
        // 注册OAuth API
        endpoints.MapOAuthApi();
        
        // 注册MCP工具API
        endpoints.MapMcpToolApi();
        
        // TODO: 注册其他API
        // endpoints.MapMcpApi();
        // endpoints.MapStatisticsApi();
        
        return endpoints;
    }
} 
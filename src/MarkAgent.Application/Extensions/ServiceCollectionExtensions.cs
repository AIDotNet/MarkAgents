using Microsoft.Extensions.DependencyInjection;
using MarkAgent.Application.Services;
using MarkAgent.Application.Services.Implementations;

namespace MarkAgent.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add application services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IConversationSessionService, ConversationSessionService>();

        return services;
    }
}
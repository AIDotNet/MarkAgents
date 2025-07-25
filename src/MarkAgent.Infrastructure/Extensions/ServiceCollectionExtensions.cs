using Microsoft.Extensions.DependencyInjection;
using MarkAgent.Domain.Repositories;
using MarkAgent.Infrastructure.Repositories;
using MarkAgent.Infrastructure.Services;
using MarkAgent.Application.Services;

namespace MarkAgent.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IConversationSessionRepository, ConversationSessionRepository>();
        services.AddScoped<IUserStatisticsRepository, UserStatisticsRepository>();

        // Add infrastructure services
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<ITodoRealtimeService, TodoRealtimeService>();

        return services;
    }
}
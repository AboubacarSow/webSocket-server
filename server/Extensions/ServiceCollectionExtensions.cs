using server.Data.Persistence;
using server.Data.Repositories;
using server.Manager;
using server.Middleware;

namespace server.Extensions;


public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<WebSocketServerMiddleware>();
        services.AddSingleton<IWebSocketServerManager,WebSocketServerManager >();
        services.AddSingleton<WebSocketDbContext>();
        services.AddScoped<IMessageRepository, MessageRepository>();

        return services;
    }
}
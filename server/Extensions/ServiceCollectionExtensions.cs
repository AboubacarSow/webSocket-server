using server.Manager;
using server.Middleware;

namespace server.Extensions;


public static class ServiceCollectionExtensions
{
    
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<WebSocketServerMiddleware>();
        services.AddSingleton<WebSocketServerManager>();

        return services;
    }
}
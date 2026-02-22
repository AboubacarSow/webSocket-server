using server.Middleware;

namespace server.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app)
    {
        app.UseMiddleware<WebSocketServerMiddleware>();

        return app;
    }
}
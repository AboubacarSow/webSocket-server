
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets();

app.Use(async (context , next) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
         await next();
    else
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

        Console.WriteLine($"Connection accepted : {webSocket.State}");
    }
});

app.Run();

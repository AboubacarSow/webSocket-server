
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets();

app.Use(async (context , next) =>
{
    WriteRequestHeaders(context.Request);
    if (!context.WebSockets.IsWebSocketRequest)
         await next();
    else
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

        Console.WriteLine($"WebSocket Connection Accepted\n Connection Status : {webSocket.State}");
    }
});

app.Run();

static void  WriteRequestHeaders(HttpRequest request)
{
    Console.WriteLine($"Request Method :{request.Method}");
    Console.WriteLine($"Request Protocol :{request.Protocol}");

    if(request.Headers is not null)
    {
        foreach(var h in request.Headers)
        {
            Console.WriteLine($"--> {h.Key} : {h.Value}");
        }
    }
}

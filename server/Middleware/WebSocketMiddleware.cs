
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using server.Manager;

namespace server.Middleware;

public class WebSocketServerMiddleware(WebSocketServerManager _webSocketManager) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.WebSockets.IsWebSocketRequest)
            await next(context);
        else
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            string connectionId = Guid.NewGuid().ToString();
            _webSocketManager.AddConnection(connectionId, webSocket);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"WebSocket Connection Accepted");
            Console.ResetColor();
            Console.WriteLine($">>> Connection ID: {connectionId}");
            //Sending connection Id to client
            var payload = JsonSerializer.Serialize(new{connectionId});
            await SendConnectionIdAsync(webSocket, payload);

            //Handling connection
            await HandleWebSocketServer(connectionId,webSocket,_webSocketManager);
        }
    }
    
    private static  async Task HandleWebSocketServer(string connectionId, WebSocket socket, WebSocketServerManager manager)
    {
        var bytes = new byte[1024 * 4];

        try
        {
            var result = await socket.ReceiveAsync(buffer: bytes,
                                                   cancellationToken: CancellationToken.None);
            //We loop until nothing is received
            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageString = Encoding.UTF8.GetString(bytes, 0, result.Count);

                    // Output message to server:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(">>> Message received");
                    Console.ResetColor();

                    Console.Write(">>> Message : ");
                    Console.WriteLine(messageString);

                    //Sending message to other clients
                    var braodcastmsg = new
                    {
                        type = "broadcast",
                        connectionId = connectionId,
                        message = messageString,
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    var payload = JsonSerializer.Serialize(braodcastmsg);
                    await manager.RoutingMsgAsync(payload, connectionId);
                }
                result = await socket.ReceiveAsync(buffer: bytes,
                                                   cancellationToken: CancellationToken.None);
            }
            if(socket.State != WebSocketState.Open)
            {
                await manager.RemoveConnectionAsyn(connectionId);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Receiving message Closed at connection with ID:{connectionId}");
                Console.WriteLine($">>> Remaining connections: {manager.GetConnectionsCount()}");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong:{ex.Message}");
            await manager.RemoveConnectionAsyn(connectionId);

        }
    }

    private static async Task SendConnectionIdAsync(WebSocket webSocket, string message)
    {
        
        var bytes = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    // private static async Task ReceiveMsgAsync(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    // {
    //     var buffer = new byte[1024 * 4];

    //     while (socket.State == WebSocketState.Open)
    //     {
    //         var result = await socket.ReceiveAsync(buffer, cancellationToken: CancellationToken.None);
    //         handleMessage(result, buffer);
    //     }
    // }

}
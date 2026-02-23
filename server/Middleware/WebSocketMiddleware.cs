
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
            var payload = JsonSerializer.Serialize(new { connectionId });
            await SendConnectionIdAsync(webSocket, payload, context.RequestAborted);

            //Handling connection
            await HandleWebSocketServer(connectionId, webSocket, _webSocketManager, context.RequestAborted);
        }
    }

    private static async Task HandleWebSocketServer(string connectionId, WebSocket socket,
     WebSocketServerManager manager, CancellationToken cancellationToken)
    {
        var bytes = new byte[1024 * 4];

        try
        {
            // Send welcome message
            var welcomeMessage = new { type = "welcome", message = $"Connection {connectionId} established!" };
            await manager.SendMsgAsync(socket, JsonSerializer.Serialize(welcomeMessage), cancellationToken);


            while (socket.State == WebSocketState.Open)
            {
                var memoryBuffer = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(bytes, cancellationToken);
                    if(result.MessageType == WebSocketMessageType.Close)
                    {
                        await manager.RemoveConnectionAsyn(connectionId,cancellationToken);
                        return;
                    }
                    if(result.MessageType!= WebSocketMessageType.Text)
                    {
                        continue;
                    }
                    memoryBuffer.Write(bytes, 0, result.Count);
                } while (!result.EndOfMessage);
                var messageString = Encoding.UTF8.GetString(memoryBuffer.ToArray());

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
                await manager.RoutingMsgAsync(payload, connectionId, cancellationToken);
            }
            //We loop until nothing is received
            await manager.RemoveConnectionAsyn(connectionId, cancellationToken);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Receiving message Closed at connection with ID:{connectionId}");
            Console.WriteLine($">>> Remaining connections: {manager.GetConnectionsCount()}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong:{ex.Message}");
            await manager.RemoveConnectionAsyn(connectionId, cancellationToken);

        }
    }

    private static async Task SendConnectionIdAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
    {
        try
        {

            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while sending message : {ex.Message}");
        }
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
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace server.Manager;


public class WebSocketServerManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public void AddConnection(string connectionId, WebSocket webSocket)
    {
        _connections.TryAdd(connectionId, webSocket);

        //logging
        Console.WriteLine($"Connection Added.");
        Console.WriteLine("----- Connection Info ----");
        Console.WriteLine($"\t ID: {connectionId}");
        Console.WriteLine($"\t Total Connections: {_connections.Count}");
    }

    public async Task RemoveConnectionAsyn(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var webSocket))
        {
            // improvement 
            try
            {
                if (webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                                statusDescription: "Connection Closed",
                                                cancellationToken: CancellationToken.None);

                    //Notifying other clients(optional)
                    var notification = new { type = "user_left", connectionId = connectionId, totalConnections = _connections.Count };
                    await RoutingMsgAsync(JsonSerializer.Serialize(notification),connectionId);
                    // logging
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Connection with ID:{connectionId} closed");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured {ex.Message}");
            }
            finally
            {
                //Avoiding leak of resources
                webSocket.Dispose();
            }
        }


    }
    //sending message to all other clients
    public async Task RoutingMsgAsync(string message, string? senderId)
    {
        //Imperative version 
        // var listOfTask = new List<Task>();
        // foreach (var conn in _connections)
        // {
        //     if (conn.Key != senderId && conn.Value.State == WebSocketState.Open)
        //     {
        //         listOfTask.Add(SendMsgAsync(conn.Value, message));
        //     }
        // }
        var listOfTask = _connections
                        .Where(conn => conn.Key != senderId &&
                                    conn.Value.State == WebSocketState.Open)
                        .Select(conn => SendMsgAsync(conn.Value, message));

        //sending message at the same time, no latency when clients receive messages
        await Task.WhenAll(listOfTask);
    }

    public async Task SendMsgAsync(WebSocket webSocket, string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);

        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"An error occured when sending message:{ex.Message}");
            Console.ResetColor();
        }
    }

    public int GetConnectionsCount() => _connections.Count;
}


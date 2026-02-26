using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using server.Models;

namespace server.Manager;

public class WebSocketServerManager : IWebSocketServerManager
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

    public async Task RemoveConnectionAsyn(string connectionId,ClosingReason reason, CancellationToken cancellationToken)
    {
        if (_connections.TryRemove(connectionId, out var webSocket))
        {
            // improvement 
            try
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State==WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(closeStatus: reason.ClosingStatus,
                                                statusDescription: reason.StatusMesssage,
                                                cancellationToken: CancellationToken.None);

                    //Notifying other clients(optional)
                    var notification = new { type = "user_left", connectionId = connectionId, totalConnections = _connections.Count };
                    await RoutingMsgAsync(JsonSerializer.Serialize(notification), connectionId, cancellationToken);
                    // logging
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Connection with ID:{connectionId} closed");
                    Console.ResetColor();
                    Console.WriteLine($">> Connections:{_connections.Count}");
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
    public async Task RoutingMsgAsync(string message, string? senderId, CancellationToken cancellationToken)
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
                        .Select(conn => SendMsgAsync(conn.Value, message, cancellationToken));

        //sending message at the same time, no latency when clients receive messages
        await Task.WhenAll(listOfTask);
    }

    public async Task SendMsgAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes),
                                    WebSocketMessageType.Text,
                                    true,
                                    cancellationToken);

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
 


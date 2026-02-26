using System.Net.WebSockets;
using server.Models;

namespace server.Manager;

public interface IWebSocketServerManager
{
    void AddConnection(string connectionId, WebSocket webSocket);
    int GetConnectionsCount();
    Task RemoveConnectionAsyn(string connectionId,ClosingReason reason, CancellationToken cancellationToken);
    Task RoutingMsgAsync(string message, string? senderId, CancellationToken cancellationToken);
    Task SendMsgAsync(WebSocket webSocket, string message, CancellationToken cancellationToken);
}
 


using System.Net.WebSockets;

namespace server.Models;

public record ClosingReason(WebSocketCloseStatus ClosingStatus,string StatusMesssage)
{
    public static ClosingReason NormalClient =>
    new (WebSocketCloseStatus.NormalClosure, "Client Requested Close Connection");
    public static ClosingReason Normal => 
    new(WebSocketCloseStatus.NormalClosure,"Connection Closing");
    public static ClosingReason InvalidValidMessageType => 
    new(WebSocketCloseStatus.InvalidMessageType,"Only text messages are allowed");
    public static ClosingReason InternalServer => 
    new (WebSocketCloseStatus.InternalServerError,"Something went wrong on the server");

    public static ClosingReason MessageToBig =>
    new (WebSocketCloseStatus.MessageTooBig, "Message Payload to big");
}

public record MessageDto (Guid Id, string ConnectionId,string Sender,string Content);


 


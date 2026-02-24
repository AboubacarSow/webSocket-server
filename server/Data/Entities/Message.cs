namespace server.Data.Entities;

public class Message
{
    public Guid Id {get;set;}
    public string? ConnectionId{get;set;}
    public string? Sender {get;set;}
    public string? Content {get;set;}
    public DateTime CreatedAt {get;set;}
}
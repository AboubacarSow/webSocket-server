using Dapper;
using server.Data.Entities;
using server.Data.Persistence;

namespace  server.Data.Repositories;

public class MessageRepository(WebSocketDbContext webSocketDbContext) : IMessageRepository
{
    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var query = @"INSERT INTO Messages (Id, ConnectionId, Sender , Content , CreatedAt)
        VALUES(@Id,@ConnectionId, @Sender,  @Content, @CreatedAt)";
        var parameter = new DynamicParameters(message);
        var connection = webSocketDbContext.DbConnection;
        await connection.ExecuteAsync(query, param:parameter);
    }

    public async Task<IEnumerable<Message>>? RetrieveAllMessagesAsync(CancellationToken cancellationToken)
    {
        var query = @"SELECT * FROM Messages ORDER BY CreatedAt DESC LIMIT 20";
        var messages = await webSocketDbContext.DbConnection.QueryAsync<Message>(query) ?? [];
        return messages;
    }
}
using server.Data.Entities;

namespace  server.Data.Repositories;

public interface IMessageRepository
{
    Task AddMessageAsync(Message message, CancellationToken cancellationToken);
    Task<IEnumerable<Message>>? RetrieveAllMessagesAsync(CancellationToken cancellationToken);
}

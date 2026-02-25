using server.Data.Entities;

namespace  server.Data.Repositories;

public interface IMessageRepository
{
    Task SaveMsgAsync(Message message, CancellationToken cancellationToken);
    Task<IEnumerable<Message>>? RetrieveAllMessagesAsync(CancellationToken cancellationToken);
}

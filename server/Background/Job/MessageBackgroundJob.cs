using Npgsql;
using server.Background.Helpers;
using server.Background.MessageQueue;
using server.Data.Entities;
using server.Data.Repositories;

namespace server.Background.Job;

//Background Worker

public class MessageBackgroundJob(IMessageQueue queue, ILogger<MessageBackgroundJob> logger,
 IServiceProvider serviceProvider) : BackgroundService
{
    private IMessageQueue _queue = queue;
    private ILogger<MessageBackgroundJob> _logger = logger;

    private IServiceProvider _serviceProvider =serviceProvider;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var mevent in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            await RetryHelper.RetryOnExceptionAsync(
            maxRetries:5,
            backoffStrategy:Backoff.LinearBackoff,
            operation: async () =>
             {
                using var scope = _serviceProvider.CreateAsyncScope();
                var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                await HandleAsync(mevent,messageRepository, stoppingToken);
             },
            isTransient: ex => ex is NpgsqlException npgex &&(npgex.IsTransient()|| npgex.SqlState =="40001"),
            logger: _logger);
            
        }

    }

    private async Task HandleAsync(MessageEvent messageEvent,IMessageRepository repository, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Id = messageEvent.Payload.Id,
            ConnectionId = messageEvent.Payload.ConnectionId,
            Sender = messageEvent.Payload.Sender,
            Content = messageEvent.Payload.Content,
            CreatedAt = messageEvent.OccurredAt
        };
        await repository.SaveMsgAsync(message, cancellationToken);
        _logger.LogInformation("Message with connection ID:{ConnectionId} saved on database", message.ConnectionId);
    }
}

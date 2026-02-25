using server.Models;

namespace server.Background.MessageQueue;

public record MessageEvent(Guid Id, MessageDto Payload, DateTime OccurredAt);
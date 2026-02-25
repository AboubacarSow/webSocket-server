using System.Threading.Channels;

namespace server.Background.MessageQueue;

public interface IMessageQueue
{
    ChannelWriter<MessageEvent> Writer { get; }
    ChannelReader<MessageEvent> Reader { get; }
}

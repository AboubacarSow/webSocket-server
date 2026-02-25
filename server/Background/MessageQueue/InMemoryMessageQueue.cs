using System.Threading.Channels;

namespace server.Background.MessageQueue;

public class InMemoryMessageQueue : IMessageQueue
{
    private readonly Channel<MessageEvent> _channel;

    public InMemoryMessageQueue()
    {
        _channel = Channel.CreateUnbounded<MessageEvent>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }
    public ChannelWriter<MessageEvent> Writer => _channel.Writer;
    public ChannelReader<MessageEvent> Reader => _channel.Reader;
}

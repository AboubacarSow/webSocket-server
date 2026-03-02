using System.Threading.Channels;

namespace server.Background.MessageQueue;

public class InMemoryMessageQueue : IMessageQueue
{
    private readonly Channel<MessageEvent> _channel;

    public InMemoryMessageQueue()
    {
        _channel = Channel.CreateBounded<MessageEvent>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true, //we have only one reader : the backgroundwork
            SingleWriter = false // each connection has its own thread , we do have multiple writer
        });
    }
    public ChannelWriter<MessageEvent> Writer => _channel.Writer;
    public ChannelReader<MessageEvent> Reader => _channel.Reader;
}

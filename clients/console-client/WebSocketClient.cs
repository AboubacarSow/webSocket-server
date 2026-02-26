using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace clients.console_client;

public class WebSocketClient(string wss_url)
{
    private ClientWebSocket? _client;
    private CancellationTokenSource? _cancellationToken;
    private string _wss_url = wss_url;
    public event Action<object?>? PrintReceivedMessage;
    public event Action? BinaryContentReceived;
    public async Task ConnectAsync()
    {
        _client = new();
        _cancellationToken = new();
        Console.WriteLine(">>> Attempting to connect to server");
        var url = new Uri(_wss_url);
        await _client.ConnectAsync(uri: url, _cancellationToken.Token);

        Console.WriteLine("Connected Successed");
        //
        _ = Task.Run(ReceiveAsync);
    }
    public async Task ReceiveAsync()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;
        MemoryStream memoryBuffer = new();
        try
        {
            while (_client?.State == WebSocketState.Open)
            {
                do
                {
                    result = await _client.ReceiveAsync(buffer, _cancellationToken.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                "Closing ...", _cancellationToken.Token);
                        return;
                    }
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        BinaryContentReceived?.Invoke();
                        await _client.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                        "Client can not handle binary message", _cancellationToken.Token);
                        return;
                    }
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        memoryBuffer.Write(buffer, 0, result.Count);
                    }


                } while (!result.EndOfMessage);

                var msJson = Encoding.UTF8.GetString(memoryBuffer.ToArray());
                memoryBuffer.SetLength(0);
                var message = JsonSerializer.Deserialize<object>(msJson);
               
                PrintReceivedMessage?.Invoke(message);
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Connection was dismissed", ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public async Task SendAsync(string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client.SendAsync(bytes, WebSocketMessageType.Text,
                             true, _cancellationToken!.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task DisconnetAsync()
    {
        try
        {
            if (_client?.State == WebSocketState.Open)
            {
                await _client.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Client request Closure", _cancellationToken!.Token);

                //disposing resources
                _cancellationToken?.Cancel();
                _cancellationToken?.Dispose();
                _client?.Dispose();
                Console.WriteLine(">>> Client Disconnected ...");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}


using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WriteLine = System.Console.WriteLine;

namespace clients.console_client;

public class WebSocketClient(string wss_url)
{
    private ClientWebSocket _client;
    private CancellationTokenSource _cancellationToken;
    private string _wss_url = wss_url;
    public event Action<object>? PrintReceivedMessage;
    public event Action? Connected;
    public event Action? Disconnected;

    public event Action<string>? BinaryContentReceived;
    public async Task ConnectAsync()
    {
        _client = new();
        _cancellationToken = new();
        Console.WriteLine(">>> Attempting to connect to server");
        var url = new Uri(_wss_url);
        await _client?.ConnectAsync(uri: url, _cancellationToken);

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
                    result = await _client?.ReceiveAsync(buffer, _cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _client?.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                "Closing ...", _cancellationToken);
                        return;
                    }
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        BinaryContentReceived?.Invoke();
                        await _client?.CloseAsync(WebSocketCloseStatus.InvalidMessageType,
                        "Client can not handle binary message", _cancellationToken);
                        return;
                    }
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        memoryBuffer.Write(buffer, 0, result.Count);
                    }


                } while (!result.EndOfMessage);

                var msJson = Encoding.UTF8.GetString(memoryBuffer.ToArray());
                var message = JsonSerializer.Deserialize(msJson);
                PrintReceivedMessage.Invoke(message);
            }
        }
        catch (OperationCanceledException ex)
        {
            WriteLine("Connection was dismissed", ex.Message);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
        }
    }
    public async Task SendAsync(string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _client?.SendAsync(bytes, WebSocketMessageType.Text,
                             true, _cancellationToken);
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
        }
    }

    public async async DisconnetAsync()
    {
        try
        {
            if (_client?.State == WebSocketState.Open)
            {
                await _client?.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Client request Closure", _cancellationToken);

                //disposing resources
                _cancellationToken?.Cancel();
                _cancellationToken?.Dispose();
                _client?.Dispose();
                WriteLine(">>> Client Disconnected ...");
            }
        }
        catch (Exception ex)
        {
            WriteLine(ex.Message);
        }
    }
}


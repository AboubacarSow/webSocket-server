// See https://aka.ms/new-console-template for more information

using clients.console_client;

var ws_url = "ws://localhost:5000";
var ws_client = new WebSocketClient(ws_url);


ws_client.PrintReceivedMessage += printer;
ws_client.BinaryContentReceived += () => Console.WriteLine("Binary message Received");

await ws_client.ConnectAsync();
Console.WriteLine("PRESS ESC TO QUIT PRGRAM");
while(Console.ReadKey().Key!= ConsoleKey.Escape)
{
    if (Console.ReadKey().Key != ConsoleKey.R)
    {
        Console.Write("\n>> Send Message:");

        var msg = Console.ReadLine();
        await ws_client.SendAsync(msg);
        continue;

    }
    await ws_client.ReceiveAsync();

}

await ws_client.DisconnetAsync();
Console.WriteLine("Program shout down ...");


void printer(object message)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(message.ToString());
    Console.ResetColor();
}







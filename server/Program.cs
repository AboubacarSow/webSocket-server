using server.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterServices();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets();

app.UseWebSocketServer();
app.Run();



// static void  WriteRequestHeaders(HttpRequest request)
// {
//     Console.WriteLine($"Request Method :{request.Method}");
//     Console.WriteLine($"Request Protocol :{request.Protocol}");

//     if(request.Headers is not null)
//     {
//         foreach(var h in request.Headers)
//         {
//             Console.WriteLine($"--> {h.Key} : {h.Value}");
//         }
//     }
// }

using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Console.WriteLine("Starting BasicSseServiceApp having SSE endpoint '/sse/counter'");

var localIP = GetLocalIPAddress();

app.MapGet("/sse/counter", async (HttpContext context) =>
{
    Console.WriteLine($"Client {context.Connection.Id} connected to SSE stream");

    context.Response.Headers.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Connection = "keep-alive";

    // counter per client stream
    var counter = 0;

    try
    {
        while (!context.RequestAborted.IsCancellationRequested)
        {
            counter++;

            var payload = new
            {
                deviceId = "SseEndpointDevice",
                timestamp = DateTime.UtcNow,
                counter
            };

            var jsonMessage = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync(jsonMessage, context.RequestAborted);
            await context.Response.Body.FlushAsync(context.RequestAborted);
            
            await Task.Delay(1000, context.RequestAborted);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Client disconnected from SSE stream");
    }
});

app.Run("http://" + localIP + ":5001");

static string GetLocalIPAddress()
{
    var host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            return ip.ToString();
        }
    }
    throw new Exception("No network adapters with an IPv4 address in the system!");
}


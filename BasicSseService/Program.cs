using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Console.WriteLine("SSE Test Server: Starting SSE endpoint '/sse/counter'");

app.MapGet("/sse/counter", async (HttpContext context, CancellationToken ct) =>
{
    Console.WriteLine($"Client {context.Connection.Id} connected to SSE stream from {context.Connection.RemoteIpAddress}");

    var lastId = -1;
    if (context.Request.Headers.TryGetValue("Last-Event-ID", out var lastEventId))
    {
        int.TryParse(lastEventId, out lastId);

        if (lastId != -1)
        {
            Console.WriteLine($"Client {context.Connection.Id} sent Last-Event-ID: {lastId}");
        }
    }

    context.Response.ContentType = "text/event-stream";
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

            await context.Response.WriteAsync($"id: {counter}\n", ct);
            await context.Response.WriteAsync($"event: counter\n", ct);

            var payload = new
            {
                deviceId = "SseEndpointDevice",
                timestamp = DateTime.UtcNow,
                counter
            };

            var jsonMessage = JsonSerializer.Serialize(payload);
            await context.Response.WriteAsync($"data: {jsonMessage}\n\n", ct);
            await context.Response.Body.FlushAsync(ct);

            await Task.Delay(1000, ct);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Client disconnected from SSE stream");
    }
});

var localIP = GetLocalIPAddress();

app.Urls.Add($"http://{localIP}:5010");
app.Urls.Add($"https://{localIP}:5011");
app.Run();

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


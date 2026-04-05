using System.Net;
using System.Net.Sockets;
using System.Text.Json;

var localIP = GetLocalIPAddress();

Console.WriteLine($"SSE Test Client - Connecting to http://{localIP}:5010/sse/counter");
Console.WriteLine("Press Ctrl+C to stop...\n");

using var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromMilliseconds(-1); // Infinite timeout for streaming

string? lastEventId = null;

try
{
    var request = new HttpRequestMessage(HttpMethod.Get, $"http://{localIP}:5010/sse/counter");

    if (!string.IsNullOrEmpty(lastEventId))
    {
        request.Headers.Add("Last-Event-ID", lastEventId);
        Console.WriteLine($"Reconnecting with Last-Event-ID: {lastEventId}");
    }

    using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

    response.EnsureSuccessStatusCode();

    Console.WriteLine($"Connected! Status: {response.StatusCode}");
    Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType}\n");

    await using var stream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);

    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        if (line.StartsWith("id: "))
        {
            lastEventId = line.Substring(4).Trim();
            Console.WriteLine($"id: {line}");
        }

        if (line.StartsWith("event: "))
        {
            Console.WriteLine($"event: {line}");
        }

        // SSE messages start with "data: "
        if (line.StartsWith("data: "))
        {
            var jsonData = line.Substring(6); // Remove "data: " prefix
            
            try
            {
                using var doc = JsonDocument.Parse(jsonData);
                var root = doc.RootElement;
                
                var deviceId = root.GetProperty("deviceId").GetString();
                var timestamp = root.GetProperty("timestamp").GetDateTime();
                var counter = root.GetProperty("counter").GetInt32();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received - Device: {deviceId}, Counter: {counter}, Timestamp: {timestamp:yyyy-MM-dd HH:mm:ss}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
                Console.WriteLine($"Raw data: {jsonData}");
            }
        }
    }
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
    Console.WriteLine("\nMake sure the BasicSseServiceApp is running on http://localhost:6000");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nDisconnected from SSE stream.");

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
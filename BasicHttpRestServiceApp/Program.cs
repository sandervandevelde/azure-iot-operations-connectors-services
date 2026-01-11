var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var counterState = new CounterState();

app.MapGet("/", () => Results.Ok(new { message = "BasicHttpRestServiceApp is running", endpoints = new[] { "/api/counter" } }));

app.MapGet("/api/counter", () =>
{
	var deviceId = "HttpRestEndpointDevice";
    var counter = Interlocked.Increment(ref counterState.Value);
	var timestamp = DateTimeOffset.UtcNow;
    Console.WriteLine($"Device {deviceId} generated counter {counter} at {timestamp}");
	return Results.Ok(new { deviceId, timestamp, counter });
});

Console.WriteLine("Starting BasicHttpRestServiceApp having Rest endpoint '/api/counter'");

app.Run();

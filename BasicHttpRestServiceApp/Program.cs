using System.Text;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var basicAuthPassword = builder.Configuration["BasicAuth:Password"];
if (string.IsNullOrWhiteSpace(basicAuthPassword))
{
	throw new InvalidOperationException("Missing required configuration 'BasicAuth:Password'.");
}

app.Use(async (context, next) =>
{
	if (context.Request.Path == "/")
	{
		await next(context);
		return;
	}

	if (!TryValidateBasicAuth(context.Request.Headers.Authorization, basicAuthPassword))
	{
		context.Response.Headers.WWWAuthenticate = "Basic realm=\"BasicHttpRestServiceApp\"";
		context.Response.StatusCode = StatusCodes.Status401Unauthorized;
		await context.Response.WriteAsync("Unauthorized");
		return;
	}

	await next(context);
});

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

var localIP = GetLocalIPAddress();

app.Run("http://" + localIP + ":5000");

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

static bool TryValidateBasicAuth(string? authorizationHeader, string requiredPassword)
{
	if (string.IsNullOrWhiteSpace(authorizationHeader))
	{
		return false;
	}

	const string prefix = "Basic ";
	if (!authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
	{
		return false;
	}

	var encoded = authorizationHeader[prefix.Length..].Trim();
	if (encoded.Length == 0)
	{
		return false;
	}

	byte[] decodedBytes;
	try
	{
		decodedBytes = Convert.FromBase64String(encoded);
	}
	catch (FormatException)
	{
		return false;
	}

	var decoded = Encoding.UTF8.GetString(decodedBytes);
	var colonIndex = decoded.IndexOf(':');
	if (colonIndex < 0)
	{
		return false;
	}

	var password = decoded[(colonIndex + 1)..];
	return password == requiredPassword;
}

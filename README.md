# Test services for Azure IoT Operations connectors

## Introduction

This repository offers test services for Azure IoT Operations connectors.

Read about Azure IoT connectors in [my blog](https://sandervandevelde.wordpress.com/)

## Services in this repo

### Basic HTTP/Rest service endpoint

This service provides a RESTful API with a '/api/counter' endpoint.

Make a call like:

```
http://[local IP address]:5000/api/counter
```

This endpoint is protected with HTTP Basic Authentication (the root endpoint is open to everyone).

*Note*: The port is hardcoded. The service does not offer its endpoints secured by TLS. You can use this example for tests only.

Example using curl:

```
curl -u anyuser:p@ssw0rd! http://[local IP address]:5000/api/counter
```

*Note*: The password is 'p@ssw0rd!' as seen in the app settings. This password is not encrypted by default. HTTP Basic Authentication is only secure when used in combination with TLS!

The response looks like:

```
{"deviceId":"HttpRestEndpointDevice","timestamp":"2026-01-11T15:06:58.442483+00:00","counter":1}
```

The counter value will be increased on every call from some service. The state (the counter) is kept in memory.

If you see calls, you know a client is connected. You can test it out using a browser as a client.

### Basic Server-sent events (SSE) service endpoint

This service provides a Server-Side Events API with a '/sse/counter' endpoint.

Make a call like:

```
http://[local IP address]:5010/sse/counter
```

This endpoint is open to everyone, no security in place.

*Note*: The port is hardcoded. The service does not offer its endpoints secured by TLS. You can use this example for tests only.

Example using curl:

```
curl http://[local IP address]:5010/sse/counter
```

The SSE response will look like:

```
id: id: 1
event: event: counter
[16:57:55] Received - Device: SseEndpointDevice, Counter: 1, Timestamp: 2026-04-06 14:57:55
id: id: 2
event: event: counter
[16:57:56] Received - Device: SseEndpointDevice, Counter: 2, Timestamp: 2026-04-06 14:57:56
id: id: 3
event: event: counter
[16:57:57] Received - Device: SseEndpointDevice, Counter: 3, Timestamp: 2026-04-06 14:57:57
```

The counter value will be increased on every call for every individual connection. The state is kept in memory. 

*Note*: The connection id is shown in the console.

#### SSE

SSE offers a continuous stream of messages, each separated with a 'newline'.

One message is a combination of three lines:

1. A line starting with 'id: '
2. A line stating with 'event: '
3. A line starting with 'data: '

A client will share the last received id as header named 'Last-Event-ID' when connecting.

*Note*: Azure IoT Operations expects all three kinds of message lines.


# Test services for Azure IoT Operations connectors

## Introduction

This repository offers test services for Azure IoT Operations connectors.

## Basic Http Rest Service

This service provides a RESTful API with a '/api/counter' endpoint.

Make a call like:

```
http://[local IP address]:5000/api/counter
```

This endpoint is protected with HTTP Basic Authentication (the root endpoint is open to everyone).

*Note*: The port is hardcoded. The service does not offer its endpoints secured by TLS. You can use this example for tests only.

Example using curl:

```
curl -u anyuser:p@ssw0rd! https://localhost:7063/api/counter
```

*Note*: The password is 'p@ssw0rd!' as seen in the app settings. This password is not encrypted by default. HTTP Basic Authentication is only secure when used in combination with TLS!

The response looks like:

```
{"deviceId":"HttpRestEndpointDevice","timestamp":"2026-01-11T15:06:58.442483+00:00","counter":1}
```

The counter value will be increased on every call. The state is kept in memory.

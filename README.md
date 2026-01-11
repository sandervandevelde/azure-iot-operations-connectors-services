# Test services for Azure IoT Operations connectors

## Introduction

This repository offers test services for Azure IoT Operations connectors.

## Basic Http Rest Service

This service offers a REST service, having a '/api/counter' endpoint.

Make a call like:

```
https://localhost:7063/api/counter
```

The response looks like:

```
{"deviceId":"HttpRestEndpointDevice","timestamp":"2026-01-11T15:06:58.442483+00:00","counter":1}
```

The counter value will be increased on every call. The state is kept in memory.


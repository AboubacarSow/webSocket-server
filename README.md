# ⏱️ Building a WebSocket Server from Scratch in .NET 9

A simple real-time communication example: a .NET 9 server that accepts WebSocket connections and a minimal web client for manual testing.

## Project Structure

```
websocket/
├── server/                 # C# .NET server application
│   ├── Program.cs          # Application entry point
│   ├── Data/               # Persistence, entities, repositories
│   ├── Extensions/         # Service & middleware extension helpers
│   ├── Manager/            # WebSocket connection manager
│   ├── Middleware/         # WebSocket middleware
│   └── Properties/         # Project properties
├── clients/                # Client applications
│   └── web-client/         # Web-based client used for manual testing
│       ├── index.html      # Main HTML
│       ├── script.js       # Client logic
│       └── style.css       # Styling
└── README.md               # This file
```

## Technology Stack

- **Backend**: C# (.NET 9)
- **Frontend**: HTML, CSS, JavaScript (vanilla)
- **Communication**: WebSocket (RFC6455)

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- A modern web browser

### Run the server

1. Open a terminal and change to the server directory:
   ```bash
   cd server
   ```

2. Restore dependencies (optional if already restored):
   ```bash
   dotnet restore
   ```

3. Run the server:
   ```bash
   dotnet run
   ```

By default the app is reachable on the local host URLs used by ASP.NET Core (you can also check `Properties/launchSettings.json`). The included web client defaults to `ws://localhost:5000`.

### Use the web client

1. Open [clients/web-client/index.html](clients/web-client/index.html) in your browser.
2. Click Connect.
3. Use the UI to send messages, observe incoming broadcasts and connection events in the communication log.

## Implemented Features

- WebSocket middleware that accepts WebSocket upgrade requests and assigns a GUID `connectionId` to each client.
- `WebSocketServerManager` to track active connections and safely broadcast messages to other clients.
- Server sends an initial payload with the assigned `connectionId` and a `welcome` message on connect.
- When a client sends a text message the server broadcasts a JSON payload to other connected clients with the shape:
  - `{ "type": "broadcast", "connectionId": "<id>", "message": "...", "timestamp": "YYYY-MM-DD HH:mm:ss" }`
- On disconnect the server notifies other clients with a `user_left` message: `{ "type": "user_left", "connectionId": "<id>", "totalConnections": N }`.
- Services and middleware are registered via `ServiceCollectionExtensions.RegisterServices()` and activated with `UseWebSocketServer()`.

- Message persistence: incoming messages are now persisted to the database using the repository layer. See key persistence files below (`Message` entity, `WebSocketDbContext`, and `IMessageRepository`/`MessageRepository`). This enables message history and future features such as history-on-connect and replay.

- Background message queue: messages are now asynchronously queued to an in-memory message channel instead of being directly persisted on the WebSocket receive path. This decouples database writes from the WebSocket middleware, improving responsiveness and preventing temporary database failures from disconnecting clients.

- Background persistence job: a hosted background service (`MessageBackgroundJob`) continuously dequeues messages from the message channel and persists them to the database with built-in **linear backoff retry logic** via `RetryHelper`. If a database write fails, the service automatically retries with configurable exponential backoff (default: starting at 1 second, configurable max retries). On repeated failures, messages are logged and eventually dropped after max attempts to prevent infinite loops.

- Close-reason support: clients and server include close reason and close status (if present) when a connection is closed.

## Message Formats

- Connection ID message (sent immediately after accept): `{ "connectionId": "<id>" }`
- Welcome message: `{ "type": "welcome", "message": "..." }`
- Broadcast message: `{ "type": "broadcast", "connectionId": "<id>", "message": "...", "timestamp": "..." }`
- User left notification: `{ "type": "user_left", "connectionId": "<id>", "totalConnections": N }`

- Note: close events may include a WebSocket close status and an optional close reason string. Clients should inspect the close info to present user-friendly messages or take reconnection decisions.

## Development Notes

- Key files:
  - [server/Manager/WebSocketManager.cs](server/Manager/WebSocketManager.cs) — manages connections and broadcasting.
  - [server/Middleware/WebSocketMiddleware.cs](server/Middleware/WebSocketMiddleware.cs) — handles accept/receive/close flow and enqueues messages to the background queue.
  - [server/Extensions/ServiceCollectionExtensions.cs](server/Extensions/ServiceCollectionExtensions.cs) — registers middleware, manager, background services, and message queue.
  - [server/Extensions/WebApplicationExtensions.cs](server/Extensions/WebApplicationExtensions.cs) — adds `UseWebSocketServer()`.
  - [server/Data/Entities/Message.cs](server/Data/Entities/Message.cs) — message entity persisted to DB.
  - [server/Data/Persistence/WebSocketDbContext.cs](server/Data/Persistence/WebSocketDbContext.cs) — EF Core DB context.
  - [server/Data/Repositories/IMessageRepository.cs](server/Data/Repositories/IMessageRepository.cs) and [server/Data/Repositories/MessageRepository.cs](server/Data/Repositories/MessageRepository.cs) — persistence abstraction and implementation.
  - [server/Background/MessageQueue/IMessageQueue.cs](server/Background/MessageQueue/IMessageQueue.cs) and [server/Background/MessageQueue/InMemoryMessageQueue.cs](server/Background/MessageQueue/InMemoryMessageQueue.cs) — in-memory message queue channel.
  - [server/Background/MessageQueue/MessageEvent.cs](server/Background/MessageQueue/MessageEvent.cs) — message event object passed through the queue.
  - [server/Background/Job/MessageBackgroundJob.cs](server/Background/Job/MessageBackgroundJob.cs) — hosted background service that persists queued messages with retry logic.
  - [server/Background/Helpers/RetryHelper.cs](server/Background/Helpers/RetryHelper.cs) — linear backoff retry utility for transient database failures.

## Project Status

- Functional: WebSocket connection handling, broadcasting, graceful disconnects, message persistence via background queue, and retry logic with linear backoff are fully implemented. The background service decouples database writes from the WebSocket middleware, ensuring that temporary database failures do not impact client connections. Manual testing via the bundled web client is supported.

## Roadmap / Planned Enhancements

The following features are planned to make the server more robust and production-ready:

- **Add message history loading on connect**: Load recent message history for clients when they connect so clients can catch up.
- **Add message ordering guarantee**: Ensure messages are delivered/processed in a well-defined order (server-side sequence numbers or persisted sequence metadata).
- **Add backpressure handling**: Detect and apply backpressure when clients or the server are overloaded (pause reads, drop/queue messages, or slow producers).
- **Use a durable background queue**: Replace the in-memory message queue with a persistent queue (e.g., RabbitMQ, Kafka, or Azure Service Bus) to ensure messages are not lost on server restarts.
- **Add rate limiting per connection**: Protect the server from abusive clients by limiting messages per connection (configurable thresholds).
- **Add authentication/authorization**: Implement JWT tokens or basic auth for secure client connections.
- **Add a console client**: Build a simple console client for testing/automation and performance benchmarking.

### Completed Features
- ✅ **Move persistence to background queue** — Messages are now dequeued from an in-memory channel by a background job, decoupling DB writes from the WebSocket middleware.
- ✅ **Add retry logic for DB failure** — Linear backoff retry strategy implemented via `RetryHelper` for transient database failures when persisting messages.

---

## References & Learning Resources

This project was inspired and informed by the following materials:

1. **RFC 6455 – The WebSocket Protocol**
   [https://datatracker.ietf.org/doc/html/rfc6455](https://datatracker.ietf.org/doc/html/rfc6455)

2. **Building Production-Ready WebSocket Servers in C# ASP.NET Core**
   [https://medium.com/@bhargavkoya56/building-production-ready-websocket-servers-in-c-asp-net-core-927b737f14cc](https://medium.com/@bhargavkoya56/building-production-ready-websocket-servers-in-c-asp-net-core-927b737f14cc)

3. **Writing a WebSocket server (MDN Web Docs)**
   [https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server)

4. **ASP.NET Core WebSockets vs SignalR – Which should you use? (Full Course) — Les Jackson**
   [https://www.youtube.com/watch?v=ycVgXe6v1VQ](https://www.youtube.com/watch?v=ycVgXe6v1VQ)

---
## License

See [LICENSE.txt](LICENSE.txt)

# WebSocket Project

A simple real-time communication example: a .NET 9 server that accepts WebSocket connections and a minimal web client for manual testing.

## Project Structure

```
websocket/
├── server/                 # C# .NET server application
│   ├── Program.cs         # Application entry point
│   ├── Extensions/        # Service & middleware extension helpers
│   ├── Manager/           # WebSocket connection manager
│   ├── Middleware/        # WebSocket middleware
│   └── Properties/        # Project properties
├── clients/               # Client applications
│   └── web-client/        # Web-based client used for manual testing
│       ├── index.html     # Main HTML
│       ├── script.js      # Client logic
│       └── style.css      # Styling
└── README.md              # This file
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

## Message Formats

- Connection ID message (sent immediately after accept): `{ "connectionId": "<id>" }`
- Welcome message: `{ "type": "welcome", "message": "..." }`
- Broadcast message: `{ "type": "broadcast", "connectionId": "<id>", "message": "...", "timestamp": "..." }`
- User left notification: `{ "type": "user_left", "connectionId": "<id>", "totalConnections": N }`

## Development Notes

- Key files:
  - [server/Manager/WebSocketManager.cs](server/Manager/WebSocketManager.cs) — manages connections and broadcasting.
  - [server/Middleware/WebSocketMiddleware.cs](server/Middleware/WebSocketMiddleware.cs) — handles accept/receive/close flow.
  - [server/Extensions/ServiceCollectionExtensions.cs](server/Extensions/ServiceCollectionExtensions.cs) — registers middleware and manager.
  - [server/Extensions/WebApplicationExtensions.cs](server/Extensions/WebApplicationExtensions.cs) — adds `UseWebSocketServer()`.

## Project Status

- Functional: basic WebSocket connection handling, broadcasting, and graceful disconnects are implemented. Ready for manual testing via the bundled web client.

## Future Enhancements

- Add authentication/authorization for connections
- Add persistence (e.g., message history) if needed
- Add console client as you can see our clients folder is in plural so, we expect to have at least two clients

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

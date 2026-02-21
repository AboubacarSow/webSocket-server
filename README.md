# WebSocket Project

A real-time communication system built with C# .NET server and a web client using WebSocket protocol.



## Project Structure

```
websocket/
├── server/                 # C# .NET server application
│   ├── Program.cs         # Application entry point
│   ├── server.csproj      # Project file
│   ├── appsettings.json   # Configuration
│   └── Properties/        # Project properties
├── clients/               # Client applications
│   └── web-client/        # Web-based client
│       ├── index.html     # Main HTML
│       ├── script.js      # Client logic
│       └── style.css      # Styling
└── README.md              # This file
```

## Technology Stack

- **Backend**: C# (.NET 9.0)
- **Frontend**: HTML, CSS, JavaScript
- **Communication**: WebSocket

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- A modern web browser

### Server Setup

1. Navigate to the server directory:
   ```bash
   cd server
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the server:
   ```bash
   dotnet run
   ```

   The server will start on the configured port (see `launchSettings.json`) under Properties directory.

### Web Client

1. Open `clients/web-client/index.html` in your web browser

2. Connect to the server using the WebSocket connection

## Project Status

⚠️ **Under Development** - The server-side implementation is currently in progress and not fully complete.

## Development Notes

- Server-side implementation is ongoing
- WebSocket endpoints are being developed
- Authentication and data handling features are planned

## Future Enhancements

- [ ] Complete WebSocket handlers
- [ ] Implement message routing
- [ ] Add error handling
- [ ] Implement authentication
- [ ] Add data persistence

## License

![License](LICENSE.txt)

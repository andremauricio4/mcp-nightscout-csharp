# C# .NET MCP Server SSE

A Model Context Protocol (MCP) server implementation using C# .NET 8.0 with Server-Sent Events (SSE) support.

## Overview

This MCP server provides tools that can be accessed via the Model Context Protocol, enabling integration with AI assistants and other MCP clients. The server runs on HTTP port 8089 and is accessible from any network interface.

## Features

- **HTTP Transport**: Uses HTTP with Server-Sent Events for communication
- **Tool Discovery**: Automatically discovers and registers tools from the current assembly
- **Greeting Tool**: Simple example tool that provides greeting functionality
- **Cross-Platform**: Built on .NET 8.0, can run on various platforms including Photon OS

## Project Structure

- `Program.cs` - Main application entry point and configuration
- `McpServer.csproj` - Project file with dependencies
- `Tools/GreetingTools.cs` - Example MCP tool implementation
- `appsettings.json` - Application configuration
- `appsettings.Development.json` - Development-specific settings

## Dependencies

- [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) (v0.3.0-preview.3)
- [ModelContextProtocol.AspNetCore](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) (v0.3.0-preview.3)

## Running the Server

1. Ensure you have .NET 8.0 SDK installed
2. Navigate to the project directory
3. Run the following commands:

```bash
dotnet restore
dotnet run
```

The server will start and listen on `http://0.0.0.0:8089`

## Available Tools

### Echo Tool
- **Description**: Says Hello to a user
- **Parameter**: `username` (string) - The name of the user to greet
- **Returns**: A greeting message

## Configuration

The server is configured to:
- Run on HTTP port 8089
- Accept connections from any network interface (0.0.0.0)
- Use HTTP transport for MCP communication
- Automatically discover tools from the current assembly

## Development

This project uses:
- .NET 8.0 as the target framework
- Nullable reference types enabled
- Implicit usings enabled
- Model Context Protocol for tool integration

## License

This project is provided as-is for demonstration and development purposes.
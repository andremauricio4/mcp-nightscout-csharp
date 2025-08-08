# C# .NET MCP Server SSE

A Model Context Protocol (MCP) server implementation using C# .NET 8.0 with Server-Sent Events (SSE) support.

## Overview

This MCP server provides tools that can be accessed via the Model Context Protocol, enabling integration with AI assistants and other MCP clients. The server runs on HTTP port 8089 and is accessible from any network interface.

## Features

- **HTTP Transport**: Uses HTTP with Server-Sent Events for communication
- **Tool Discovery**: Automatically discovers and registers tools from the current assembly
- **Cross-Platform**: Built on .NET 8.0, can run on various platforms including Linux

## Project Structure

- `Program.cs` - Main application entry point and configuration
- `McpServer.csproj` - Project file with dependencies
- `Tools/*Tool.cs` - MCP tool implementation
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

The server auto-discovers MCP tools from the current assembly. Below are the available tools (tool names match method names):

| Tool | Description | Parameters | Returns |
| --- | --- | --- | --- |
| `GetBloodGlucose` | Gets the past blood glucose values (mg/dL) | `count: int` (default: 12) | `string` (human-readable table grouped by date) |
| `DeleteTreatment` | Deletes a treatment by its ID | `_id: string` | `string` (success/failure message) |
| `GetBasalInsulin` | Gets latest basal (slow-acting) insulin records | `count: int` (default: 12) | `string` (grouped by date) |
| `GetFingerPrickCapillaryGlucometerChecks` | Gets finger-prick/capillary glucometer BG checks | `count: int` (default: 12) | `string` (grouped by date) |
| `GetInsulinBoluses` | Gets latest bolus insulin records | `count: int` (default: 12) | `string` (grouped by date) |
| `GetExercise` | Gets logged exercise records | `count: int` (default: 12) | `string` (grouped by date) |
| `GetMeals` | Gets logged meals (carbs and description) | `count: int` (default: 12) | `string` (grouped by date) |
| `GetNotes` | Gets logged notes | `count: int` (default: 12) | `string` (grouped by date) |
| `GetSensorStart` | Gets dates/times when sensors were started | `months: int` (default: 1) | `string` (per-event lines) |
| `RecordBasalInsulin` | Records basal insulin administration | `absolute: double?` (units), `duration: int?` (default: 1440), `notesDescription: string?`, `eventTime: string?` (`yyyy-MM-dd HH:mm`, Europe/Lisbon) | `string` (success/failure with treatment ID) |
| `RecordBolusInsulin` | Records bolus insulin administration | `insulin: double?`, `notesDescription: string?`, `eventTime: string?` | `string` (success/failure with treatment ID) |
| `RecordExercise` | Records exercise activity | `duration: int?`, `notesDescription: string?`, `eventTime: string?` | `string` (success/failure with treatment ID) |
| `RecordFingerPrickCapillaryGlucometerCheck` | Records finger-prick/capillary glucometer BG check | `glucose: int` (mg/dL), `eventTime: string?` | `string` (success/failure with treatment ID) |
| `RecordFood` | Records food intake with carbs | `carbs_g: double?`, `notesDescription: string?`, `eventTime: string?` | `string` (success/failure with treatment ID) |
| `RecordNotes` | Records a note/event | `notesDescription: string`, `eventTime: string?` | `string` (success/failure with treatment ID) |

Notes:
- All returned values are human-readable strings optimized for chat display.
- Where applicable, quantities use mg/dL for glucose and international units (u) for insulin.
- `eventTime` parameters are optional and, when provided, must be in `yyyy-MM-dd HH:mm` using Europe/Lisbon local time.

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

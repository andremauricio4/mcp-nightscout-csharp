using Services;

var builder = WebApplication.CreateBuilder(args);

// Configure to run only on HTTP port 8089 (accessible from any network interface)
builder.WebHost.UseUrls("http://0.0.0.0:8089");

// Register HttpClient and NightscoutService
builder.Services.AddHttpClient<NightscoutService>();

// Register MCP server and discover tools from the current assembly
builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly();
var app = builder.Build();
// Add MCP middleware
app.MapMcp();

// Handle graceful shutdown
var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStopping.Register(() => {
    Console.WriteLine("Application is shutting down gracefully...");
});

app.Run();
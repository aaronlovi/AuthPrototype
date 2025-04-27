using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AuthPrototype.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OAuthToolkit;
using Serilog;
using WebSocketToolkit;

namespace AuthPrototype;

public class Program {
    public static void Main(string[] args) {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try {
            Log.Information("Starting up the application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            _ = builder.Host.UseSerilog();

            _ = builder.Services.
                AddSingleton(svp => {
                    ILogger<UsersFileStore> logger = svp.GetRequiredService<ILogger<UsersFileStore>>();
                    return new UsersFileStore("users.txt", logger);
                }).
                AddControllers().
                AddJsonOptions(options => {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                });

            _ = builder.Services.ConfigureOAuthToolkit(builder.Configuration);

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
                _ = app.UseDeveloperExceptionPage();

            ConfigureWebSocketEndpoints(app);

            _ = app.UseHttpsRedirection();
            _ = app.UseAuthorization();
            _ = app.MapControllers();

            _ = app.MapGet("/", () => $"AuthPrototype in {app.Environment.EnvironmentName} mode");

            app.Run();
        } catch (Exception ex) {
            Log.Fatal(ex, "Application start-up failed");
        } finally {
            Log.CloseAndFlush();
        }
    }

    private static void ConfigureWebSocketEndpoints(WebApplication app) {
        // Enable WebSockets
        _ = app.UseWebSockets();

        // Map WebSocket endpoint
        _ = app.Map("/ws", async context => {
            if (!context.WebSockets.IsWebSocketRequest) {
                context.Response.StatusCode = 400;
                return;
            }

            using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var wsServer = new WebSocketServer();
            wsServer.OnConnected += async (ws) => {
                Log.Logger.Information("WebSocket connected: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
                await Task.CompletedTask;
            };
            wsServer.OnDisconnected += async (ws) => {
                Log.Logger.Information("WebSocket disconnected: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
                await Task.CompletedTask;
            };
            wsServer.OnMessageReceived += async (ws, msg) => {
                Log.Logger.Information("WebSocket message received: {Msg}", msg);
                // Echo message back
                byte[] buffer = Encoding.UTF8.GetBytes($"Echo: {msg}");
                await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            };
            await wsServer.HandleWebSocketAsync(webSocket, context.RequestAborted);
        });
    }
}

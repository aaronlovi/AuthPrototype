using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketToolkit;

/// <summary>
/// Provides basic WebSocket server functionality, including connection, disconnection, and message handling events.
/// Intended for use as reusable library code in backend projects.
/// </summary>
public class WebSocketServer {
    /// <summary>
    /// Event triggered when a new WebSocket connection is established.
    /// </summary>
    public event Func<WebSocket, Task>? OnConnected;

    /// <summary>
    /// Event triggered when a WebSocket connection is closed.
    /// </summary>
    public event Func<WebSocket, Task>? OnDisconnected;

    /// <summary>
    /// Event triggered when a message is received from a WebSocket client.
    /// </summary>
    public event Func<WebSocket, string, Task>? OnMessageReceived;

    /// <summary>
    /// Handles the WebSocket session lifecycle for a single connection.
    /// Invokes connection, message, and disconnection events as appropriate.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection to handle.</param>
    /// <param name="ct">Cancellation token for the session.</param>
    public async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken ct) {
        await (OnConnected?.Invoke(webSocket) ?? Task.CompletedTask);
        byte[] buffer = new byte[4096];
        try {
            while (webSocket.State == WebSocketState.Open && !ct.IsCancellationRequested) {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                string message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (OnMessageReceived != null)
                    await OnMessageReceived(webSocket, message);
            }
        } finally {
            await (OnDisconnected?.Invoke(webSocket) ?? Task.CompletedTask);
            if (webSocket.State == WebSocketState.Open) {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
            }
        }
    }
}

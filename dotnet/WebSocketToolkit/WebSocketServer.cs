using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketToolkit;

public class WebSocketServer {
    // Event for when a new connection is established
    public event Func<WebSocket, Task>? OnConnected;
    // Event for when a connection is closed
    public event Func<WebSocket, Task>? OnDisconnected;
    // Event for when a message is received
    public event Func<WebSocket, string, Task>? OnMessageReceived;

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

using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketManager;

public class CustomWebSocketHandler : WebSocketHandler
{
    public CustomWebSocketHandler(WebSocketConnectionManager webSocketConnectionManager) : base(webSocketConnectionManager)
    {
    }

    public override async Task OnConnected(WebSocket socket)
    {
        await base.OnConnected(socket);
        var socketId = WebSocketConnectionManager.GetId(socket);
        await SendMessageAsync(socket, $"Connected with ID: {socketId}");
    }

    public override async Task OnDisconnected(WebSocket socket)
    {
        await base.OnDisconnected(socket);
    }

    public async Task SendMessageToAllAsync(string message)
    {
        foreach (var pair in WebSocketConnectionManager.GetAll())
        {
            if (pair.Value.State == WebSocketState.Open)
            {
                await SendMessageAsync(pair.Value, message);
            }
        }
    }

    public async Task SendMessageAsync(WebSocket socket, string message)
    {
        if (socket.State != WebSocketState.Open)
            return;

        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task HandleWebRTCSignaling(WebSocket socket, string message)
    {
        // Handle WebRTC signaling messages (offer, answer, ICE candidates)
        await SendMessageToAllAsync(message);
    }
}

public static class WebSocketManagerExtensions
{
    public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app, PathString path, CustomWebSocketHandler handler)
    {
        return app.Map(path, (_app) => _app.UseMiddleware<WebSocketManagerMiddleware>(handler));
    }
}

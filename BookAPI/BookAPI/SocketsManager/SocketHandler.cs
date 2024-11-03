using System.Net.WebSockets;
using System.Text;
using BookAPI.SocketsManager;

namespace BookAPI.SocketsManager
{
    public abstract class SocketHandler
    {
        public ConnectionManager Connections { get; set; }
        public SocketHandler(ConnectionManager connections)
        {
            Connections = connections;
        }

        public virtual async Task OnConnected(WebSocket socket)
        {
            await Task.Run(() => { Connections.AddSocket(socket); });
        }

        public virtual async Task OnDisconnected(WebSocket socket)
        {
            await Connections.RemoveSocketAsync(Connections.GetId(socket));
        }

        public async Task SendMessage(WebSocket socket, string message)
        {
            if (socket.State !== WebSocketState.Open)
                return;
            await socket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length), WebSocketMessageType.Text, CancellationToken.None);
        }
    }
}

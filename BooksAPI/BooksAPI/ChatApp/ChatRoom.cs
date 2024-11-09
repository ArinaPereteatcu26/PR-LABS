using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace BooksAPI.ChatApp
{
    [EnableCors("AllowSpecificOrigins")]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client Connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client Disconnected: {Context.ConnectionId}");
            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
                await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"A user has joined {roomName}");
                _logger.LogInformation($"User {Context.ConnectionId} joined room {roomName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining room {roomName}");
                throw;
            }
        }

        public async Task LeaveRoom(string roomName)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
                await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"A user has left {roomName}");
                _logger.LogInformation($"User {Context.ConnectionId} left room {roomName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving room {roomName}");
                throw;
            }
        }

        public async Task SendMessage(string roomName, string user, string message)
        {
            try
            {
                await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message);
                _logger.LogInformation($"Message sent in {roomName} by {user}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message in room {roomName}");
                throw;
            }
        }
    }
}
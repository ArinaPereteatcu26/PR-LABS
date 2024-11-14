using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;

namespace BooksAPI.Hubs;

[EnableCors("AllowSpecificOrigins")]
public class ChatHub(ILogger<ChatHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation($"Client Connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation($"Client Disconnected: {Context.ConnectionId}");
        if (exception != null) logger.LogError(exception, "Client disconnected with error");

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomName)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"A user has joined {roomName}");
            logger.LogInformation($"User {Context.ConnectionId} joined room {roomName}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error joining room {roomName}");
            throw;
        }
    }

    public async Task LeaveRoom(string roomName)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"A user has left {roomName}");
            logger.LogInformation($"User {Context.ConnectionId} left room {roomName}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error leaving room {roomName}");
            throw;
        }
    }

    public async Task SendMessage(string roomName, string user, string message)
    {
        try
        {
            await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message);
            logger.LogInformation($"Message sent in {roomName} by {user}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error sending message in room {roomName}");
            throw;
        }
    }
}
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using BookAPI.Data;
using BookAPI.ChatRoomApp;
using BookAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
});

// Add WebSocket Manager service
builder.Services.AddSingleton<ChatRoom>(); // Register ChatRoom as a singleton
builder.Services.AddWebSocketManager(); // Ensure you have this extension method available in your project

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

// Enable developer exception page in development mode
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable middleware for serving generated Swagger as a JSON endpoint
app.UseSwagger();

// Enable middleware for serving Swagger UI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
});

// Enable WebSockets
app.UseWebSockets();

// Map WebSocket handler
app.Map("/ws", async (HttpContext context, ChatRoom chatRoom) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        chatRoom.AddConnection(webSocket); // Add the connection to the chat room
    }
    else
    {
        context.Response.StatusCode = 400; // Bad request
    }
});

// Enable routing
app.UseRouting();
app.UseAuthorization();

// Map your controllers
app.MapControllers();

var webSocketRoom = new ChatRoom();
var webSocketMiddleware = new WebSocketMiddleware();
Task.Run(() => webSocketMiddleware.StartWebSocketServer(webSocketRoom));

app.Run();

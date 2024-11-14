using BooksAPI;
using BooksAPI.Data;
using BooksAPI.Hubs;
using BooksAPI.Repositories;
using BooksAPI.TCP;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<TcpServerIo>(provider =>
    new TcpServerIo(
        provider.GetRequiredService<ILogger<TcpServerIo>>()
    ));
builder.Services.AddHostedService<TcpServerIo>();
var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

await DatabaseMigrator.EnsureDatabase(builder.Configuration.GetConnectionString("DefaultConnection")!);
DatabaseMigrator.RunMigrations(app, builder.Configuration.GetConnectionString("DefaultConnection")!);

app.Run();

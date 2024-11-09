using BooksAPI.Data;
using BooksAPI.ChatApp;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:5147", "https://localhost:7053")
              .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials(); 
    });
});


builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowSpecificOrigins");
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
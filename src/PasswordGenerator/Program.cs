using Microsoft.EntityFrameworkCore;
using PasswordGenerator.Api;
using PasswordGenerator.Application;
using PasswordGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Passwords") ?? "Data Source=passwords.db";

builder.Services.AddDbContext<PasswordDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<IPasswordGenerator, StrongPasswordGenerator>();
builder.Services.AddSingleton<IGuidGenerator, SystemGuidGenerator>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<PasswordDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.MapPasswordEndpoints();

app.Run();

public partial class Program;

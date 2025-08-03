using Aikido.Data;
using Aikido.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

Directory.CreateDirectory("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var connString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

builder.Services.AddScoped<IUserService, UserService>();
    .AddScoped<ClubService>()
    .AddScoped<GroupService>()
    .AddScoped<TableService>()
    .AddScoped<ScheduleService>()
    .AddScoped<AttendanceService>()
    .AddScoped<SeminarService>()
    .AddScoped<PaymentService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

app.MapGet("/", () => "Сервер работает!");
app.UseCors("AllowAll");
app.MapControllers();

app.Run("http://0.0.0.0:5000");

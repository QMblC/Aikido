using Aikido.Data;
using Aikido.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<UserService>()
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

app.MapGet("/", () => "������ ��������!");
app.UseCors("AllowAll");
app.MapControllers();
app.Run("http://0.0.0.0:5000");

using Aikido.Application.Services;
using Aikido.Data;
using Aikido.Services;
using Aikido.Services.DatabaseServices;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.UnitOfWork;
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

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

builder.Services.AddScoped<IUserDbService, UserDbService>();
builder.Services.AddScoped<IClubDbService, ClubDbService>();
builder.Services.AddScoped<IGroupDbService, GroupDbService>();
builder.Services.AddScoped<ISeminarDbService, SeminarDbService>();

builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ScheduleService>();

builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<TableService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<UserApplicationService>();
builder.Services.AddScoped<ClubApplicationService>();
builder.Services.AddScoped<GroupApplicationService>();
builder.Services.AddScoped<SeminarApplicationService>();
builder.Services.AddScoped<EventApplicationService>();
builder.Services.AddScoped<AttendanceApplicationService>();
builder.Services.AddScoped<PaymentApplicationService>();
builder.Services.AddScoped<ScheduleApplicationService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Aikido Club Management API",
        Version = "v1",
        Description = "API для управления клубами айкидо с поддержкой 3НФ"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aikido API v1");
        c.RoutePrefix = "swagger";
    });
}

app.MapGet("/", () => "Сервер Aikido работает! Перейдите на /swagger для API документации");

app.MapGet("/health", async (AppDbContext context) =>
{
    try
    {
        await context.Database.CanConnectAsync();
        return Results.Ok(new { Status = "Healthy", Database = "Connected", Timestamp = DateTime.UtcNow });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.UseCors("AllowAll");
app.MapControllers();

Log.Information("Запуск сервера Aikido на http://0.0.0.0:5000");
app.Run("http://0.0.0.0:5000");
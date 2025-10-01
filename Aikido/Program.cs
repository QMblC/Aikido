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

// Создание директории для логов
Directory.CreateDirectory("logs");

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Использование Serilog
builder.Host.UseSerilog();

// Получение строки подключения
var connString = builder.Configuration.GetConnectionString("DefaultConnection");

// Настройка CORS
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

// Настройка Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

// Регистрация Database Services (слой доступа к данным)
builder.Services.AddScoped<IUserDbService, UserDbService>();
builder.Services.AddScoped<IClubDbService, ClubDbService>();
builder.Services.AddScoped<IGroupDbService, GroupDbService>();
builder.Services.AddScoped<ISeminarDbService, SeminarDbService>();

// Регистрация остальных Database Services
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ScheduleService>();

// Регистрация других сервисов
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<TableService>();

// Регистрация UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// РЕГИСТРАЦИЯ APPLICATION SERVICES (новый слой)
builder.Services.AddScoped<UserApplicationService>();
builder.Services.AddScoped<ClubApplicationService>();
builder.Services.AddScoped<GroupApplicationService>();
builder.Services.AddScoped<SeminarApplicationService>();
builder.Services.AddScoped<EventApplicationService>();
builder.Services.AddScoped<AttendanceApplicationService>();
builder.Services.AddScoped<PaymentApplicationService>();
builder.Services.AddScoped<ScheduleApplicationService>();

// Настройка Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Настройка Swagger для разработки
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

// Настройка конвейера обработки HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aikido API v1");
        c.RoutePrefix = "swagger";
    });
}

// Простой endpoint для проверки работы сервера
app.MapGet("/", () => "Сервер Aikido работает! Перейдите на /swagger для API документации");

// Endpoint для проверки состояния БД
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

// Использование CORS
app.UseCors("AllowAll");

// Маршрутизация контроллеров
app.MapControllers();

// Запуск приложения
Log.Information("Запуск сервера Aikido на http://0.0.0.0:5000");
app.Run("http://0.0.0.0:5000");
using Aikido.Application.Services;
using Aikido.Configuration;
using Aikido.Data;
using Aikido.Middleware;
using Aikido.Services;
using Aikido.Services.DatabaseServices;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.Seminar;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


Directory.CreateDirectory("logs");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AccessToken"];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOriginsWithCredentials", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var connString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connString));

builder.Services.AddScoped<IUserDbService, UserDbService>();
builder.Services.AddScoped<IClubDbService, ClubDbService>();
builder.Services.AddScoped<IGroupDbService, GroupDbService>();
builder.Services.AddScoped<ISeminarDbService, SeminarDbService>();

builder.Services.AddScoped<AttendanceDbService>();
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
builder.Services.AddScoped<UserChangeRequestDbService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthApplicationService>();
builder.Services.AddScoped<ISeminarTrainerEditRequestDbService, SeminarTrainerEditRequestDbService>();
builder.Services.AddScoped<SeminarTrainerEditRequestAppService>();



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.JsonSerializerOptions.MaxDepth = 32;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Aikido Club Management API",
        Version = "v1",
        Description = "API для управления клубами айкидо"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
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

app.UseCors("AllowAllOriginsWithCredentials");
app.UseCookiePolicy();

app.UseAuthentication();
app.UseJwtCookie();
app.UseAuthorization();
app.MapControllers();

Log.Information("Запуск сервера Aikido на http://0.0.0.0:5000");
app.Run("http://0.0.0.0:5000");
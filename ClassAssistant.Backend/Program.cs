using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Hubs;
using ClassAssistant.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});

// Configure CORS for Blazor frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure EF Core with PostgreSQL (or In-Memory Fallback if Postgres is offline)
string connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=classassistant_db;Username=postgres;Password=postgres";

bool isPostgresAvailable = false;
try
{
    var builderConn = new Npgsql.NpgsqlConnectionStringBuilder(connString) { Timeout = 2 };
    using var testConn = new Npgsql.NpgsqlConnection(builderConn.ConnectionString);
    testConn.Open();
    isPostgresAvailable = true;
    Console.WriteLine("[Database SUCCESS] Connected to PostgreSQL database server.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Database INFO] PostgreSQL server is offline or unreachable ({ex.Message}). Using In-Memory database fallback.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (isPostgresAvailable)
    {
        options.UseNpgsql(connString);
    }
    else
    {
        options.UseInMemoryDatabase("ClassAssistantDb");
    }
});

// Configure JWT Authentication
string jwtSecret = builder.Configuration["Jwt:Secret"] ?? "ClassAssistantSuperSecretJwtSigningKey2026!@#$%^&*()_+";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Register Application Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddSingleton<IFaceRecognitionService, FaceRecognitionService>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

// Seed Default PostgreSQL Data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DbInitializer.Initialize(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DbInitializer WARNING] Could not seed database: {ex.Message}");
    }
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AttendanceHub>("/hubs/attendance");

app.Run();

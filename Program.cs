using AIChatBot.Data;
using AIChatBot.Middleware;
using AIChatBot.Repositories;
using AIChatBot.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

foreach (var source in builder.Configuration.Sources
    .OfType<Microsoft.Extensions.Configuration.FileConfigurationSource>())
{
    source.ReloadOnChange = false;
    source.ReloadDelay = 0;
}

// ---------------- MONGODB CONFIG ----------------

var mongoConnection =
    Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
    ?? builder.Configuration["MongoSettings:ConnectionString"];

var mongoDbName =
    Environment.GetEnvironmentVariable("MONGO_DB")
    ?? builder.Configuration["MongoSettings:DatabaseName"];

if (string.IsNullOrEmpty(mongoConnection) || string.IsNullOrEmpty(mongoDbName))
{
    throw new Exception("MongoDB configuration is missing");
}

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = mongoConnection;
    options.DatabaseName = mongoDbName;
});

// ---------------- JWT CONFIG ----------------

var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new Exception("JWT Secret is missing");
}

builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = jwtSecret;
    options.ExpiryMinutes = builder.Configuration.GetValue<int>("JwtSettings:ExpiryMinutes");
});

// Secure key
var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);
if (keyBytes.Length < 32)
{
    using var sha = SHA256.Create();
    keyBytes = sha.ComputeHash(keyBytes);
}

// ---------------- SERVICES ----------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://aichat-frontend-e1iz.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Repositories
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IChatRepository, ChatRepository>();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAIService, AIService>();

builder.Services.AddHttpClient();

// ---------------- AUTH ----------------

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Read token from cookie
            var token = context.Request.Cookies["token"];

            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }
    };
});


// Middleware
builder.Services.AddSingleton<ErrorHandlingMiddleware>();

// ---------------- APP ----------------

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Render port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
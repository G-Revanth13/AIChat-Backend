////using AIChatBot.Data;
////using AIChatBot.Middleware;
////using AIChatBot.Repositories;
////using AIChatBot.Services;
////using Microsoft.AspNetCore.Authentication.JwtBearer;
////using Microsoft.IdentityModel.Tokens;
////using System.Text;
////using System.Security.Cryptography;

////var builder = WebApplication.CreateBuilder(args);

////// Configuration
////builder.Services.Configure<MongoDbSettings>(
////    builder.Configuration.GetSection("MongoDb"));

////builder.Services.Configure<JwtSettings>(
////    builder.Configuration.GetSection("Jwt"));

////// Controllers
////builder.Services.AddControllers();

////// Swagger
////builder.Services.AddEndpointsApiExplorer();
////builder.Services.AddSwaggerGen();

////// CORS
////builder.Services.AddCors(options =>
////{
////    options.AddPolicy("AllowFrontend", policy =>
////    {
////        policy.WithOrigins(
////                "http://localhost:5173",
////                "https://your-frontend.onrender.com"
////            )
////            .AllowAnyHeader()
////            .AllowAnyMethod()
////            .AllowCredentials();
////    });
////});

////// Repositories
////builder.Services.AddSingleton<IUserRepository, UserRepository>();
////builder.Services.AddSingleton<IChatRepository, ChatRepository>();

////// Services
////builder.Services.AddScoped<IJwtService, JwtService>();
////builder.Services.AddScoped<IUserService, UserService>();
////builder.Services.AddScoped<IChatService, ChatService>();
////builder.Services.AddScoped<IAIService, AIService>();

////builder.Services.AddHttpClient();

////// JWT
////var jwtSection = builder.Configuration.GetSection("JwtSettings");
////var key = jwtSection.GetValue<string>("Secret");

////if (string.IsNullOrEmpty(key))
////{
////    throw new Exception("JWT Key is missing");
////}

////var keyBytes = Encoding.UTF8.GetBytes(key);
////if (keyBytes.Length < 32)
////{
////    using var sha = SHA256.Create();
////    keyBytes = sha.ComputeHash(keyBytes);
////}

////builder.Services.AddAuthentication(options =>
////{
////    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
////    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
////})
////.AddJwtBearer(options =>
////{
////    options.RequireHttpsMetadata = false;
////    options.SaveToken = true;

////    options.TokenValidationParameters = new TokenValidationParameters
////    {
////        ValidateIssuer = false,
////        ValidateAudience = false,
////        ValidateLifetime = true,
////        ValidateIssuerSigningKey = true,
////        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
////    };
////});

////// Middleware
////builder.Services.AddSingleton<ErrorHandlingMiddleware>();

////var app = builder.Build();

////// Swagger
////if (app.Environment.IsDevelopment())
////{
////    app.UseSwagger();
////    app.UseSwaggerUI();
////}

////// Middleware
////app.UseMiddleware<ErrorHandlingMiddleware>();

////// CORS
////app.UseCors("AllowFrontend");

////// Auth
////app.UseAuthentication();
////app.UseAuthorization();

////app.MapControllers();

////// Render Port
////var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
////app.Urls.Add($"http://0.0.0.0:{port}");

////app.Run();


////deployed
//using AIChatBot.Data;
//using AIChatBot.Middleware;
//using AIChatBot.Repositories;
//using AIChatBot.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using System.Security.Cryptography;

//var builder = WebApplication.CreateBuilder(args);

//// ---------------- CONFIG ----------------

//// MongoDB (env first → fallback)
//var mongoConnection =
//    Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
//    ?? builder.Configuration["MongoDb:ConnectionString"];

//var mongoDbName =
//    Environment.GetEnvironmentVariable("MONGO_DB")
//    ?? builder.Configuration["MongoDb:DatabaseName"];

//builder.Services.Configure<MongoDbSettings>(options =>
//{
//    options.ConnectionString = mongoConnection;
//    options.DatabaseName = mongoDbName;
//});

//// JWT (env first → fallback)
//var jwtSection = builder.Configuration.GetSection("JwtSettings");

//var jwtSecret =
//    Environment.GetEnvironmentVariable("JWT_SECRET")
//    ?? jwtSection.GetValue<string>("Secret");

//if (string.IsNullOrEmpty(jwtSecret))
//{
//    throw new Exception("JWT Secret is missing");
//}

//// Secure key
//var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);
//if (keyBytes.Length < 32)
//{
//    using var sha = SHA256.Create();
//    keyBytes = sha.ComputeHash(keyBytes);
//}

//// ---------------- SERVICES ----------------

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// CORS (IMPORTANT FIX)
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", policy =>
//    {
//        policy.WithOrigins(
//                "http://localhost:5173",
//                "https://aichatbot-frontend.onrender.com" // 🔁 replace if different
//            )
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials();
//    });
//});

//// Repos
//builder.Services.AddSingleton<IUserRepository, UserRepository>();
//builder.Services.AddSingleton<IChatRepository, ChatRepository>();

//// Services
//builder.Services.AddScoped<IJwtService, JwtService>();
//builder.Services.AddScoped<IUserService, UserService>();
//builder.Services.AddScoped<IChatService, ChatService>();
//builder.Services.AddScoped<IAIService, AIService>();

//builder.Services.AddHttpClient();

//// JWT Auth
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.RequireHttpsMetadata = false;
//    options.SaveToken = true;

//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
//    };
//});

//// Middleware
//builder.Services.AddSingleton<ErrorHandlingMiddleware>();

//// ---------------- APP ----------------

//var app = builder.Build();

//// Swagger (keep enabled for debugging Render)
//app.UseSwagger();
//app.UseSwaggerUI();

//// Middleware
//app.UseMiddleware<ErrorHandlingMiddleware>();

//// ❌ DO NOT enable HTTPS redirect on Render
//// app.UseHttpsRedirection();

//// CORS
//app.UseCors("AllowFrontend");

//// Auth
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//// Render port binding
//var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
//app.Urls.Add($"http://0.0.0.0:{port}");

//app.Run();

using AIChatBot.Data;
using AIChatBot.Middleware;
using AIChatBot.Repositories;
using AIChatBot.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// ---------------- MONGODB CONFIG ----------------

// ✅ FIXED: use correct key "MongoSettings"
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

// Register strongly typed settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Read secret (env first → fallback)
var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["JwtSettings:Secret"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new Exception("JWT Secret is missing");
}

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
                "https://aichatbot-frontend.onrender.com"
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

    // ✅ ADD THIS (FIX FOR YOUR ISSUE)
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
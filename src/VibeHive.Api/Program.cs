// Yevgeniy Mazur
// API setup and JWT security.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VibeHive.Api.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var secret = builder.Configuration["Jwt:Key"];

if (secret is not null
    && Encoding.UTF8.GetByteCount(secret) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must be at least 32 UTF-8 bytes.");
}

var keyBytes = secret is null
    ? RandomNumberGenerator.GetBytes(32)
    : Encoding.UTF8.GetBytes(secret);

var key = new SymmetricSecurityKey(keyBytes);

builder.Services.AddSingleton(new TokenService(key));
builder.Services.AddSingleton<EventStore>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "VibeHive",

                ValidateAudience = true,
                ValidAudience = "VibeHive.Desktop",

                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                ClockSkew = TimeSpan.FromSeconds(15)
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    service = "VibeHive API",
    status = "ready"
}));

app.MapControllers();

app.Run();

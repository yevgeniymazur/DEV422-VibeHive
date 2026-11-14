// Yevgeniy Mazur
// API setup.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VibeHive.Api.Events;
using VibeHive.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var secret = builder.Configuration["Jwt:Key"];
if (secret is not null && Encoding.UTF8.GetByteCount(secret) < 32) throw new InvalidOperationException("Jwt:Key must be at least 32 UTF-8 bytes.");
var key = new SymmetricSecurityKey(secret is null ? RandomNumberGenerator.GetBytes(32) : Encoding.UTF8.GetBytes(secret));
builder.Services.AddSingleton(new TokenService(key));
builder.Services.AddSingleton<EventStore>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = "VibeHive",
        ValidateAudience = true, ValidAudience = "VibeHive.Desktop",
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        IssuerSigningKey = key, ClockSkew = TimeSpan.FromSeconds(15)
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
var email = builder.Configuration["Organizer:Email"];
var password = builder.Configuration["Organizer:Password"];
if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
{
    if (password.Length < 8) throw new InvalidOperationException("Organizer password must contain at least 8 characters.");
    var store = app.Services.GetRequiredService<EventStore>();
    var account = new Account { User = new(Guid.NewGuid(), "Event Organizer", email.Trim().ToLowerInvariant(), "Organizer") };
    account.PasswordHash = store.Hasher.HashPassword(account, password);
    store.Users.Add(account);
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { service = "VibeHive API", status = "ready" }));
app.MapControllers();
app.Run();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { service = "VibeHive API", status = "ready" }));
// Eugene Mazur owns registration, login, JWT, roles, events, and ticketing.
// TODO: Configure JWT authentication and authorization before adding protected endpoints.
app.MapControllers();
app.Run();

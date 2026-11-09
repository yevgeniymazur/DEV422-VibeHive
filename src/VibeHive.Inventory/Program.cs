using VibeHive.Inventory.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
var app = builder.Build();
app.MapGrpcService<AlbumInventoryService>();
app.MapGet("/", () => "VibeHive Album Inventory: connect using a gRPC client.");
app.Run();

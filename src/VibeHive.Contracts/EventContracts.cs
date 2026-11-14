// Yevgeniy Mazur
// Event and ticket models.

using System.ComponentModel.DataAnnotations;

namespace VibeHive.Contracts;

public static class UserRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
    public const string EventManager = "Event Manager";
}

public static class EventStatuses
{
    public const string Pending = "Pending";
    public const string AwaitingManager = "Awaiting Manager Confirmation";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class TicketStatuses
{
    public const string Booked = "Booked";
    public const string Cancelled = "Cancelled";
}

public record VenueDto(string Id, string Name, int MaxCapacity);

public static class VenueCatalog
{
    public static IReadOnlyList<VenueDto> Venues { get; } =
        new List<VenueDto>
        {
            new("crocodile", "The Crocodile", 750),
            new("showbox", "The Showbox", 1100),
            new("showbox-sodo", "Showbox SoDo", 1800),
            new("wamu", "WAMU Theater", 9000),
            new("tacoma-dome", "Tacoma Dome", 22000)
        }.AsReadOnly();
}

public record RegisterRequest(
    [Required, StringLength(80, MinimumLength = 2)] string Name,
    [Required, EmailAddress] string Email,
    [Required, StringLength(128, MinimumLength = 8)] string Password,
    string Role = UserRoles.User);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role);

public record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    UserDto User);

public record EventSessionRequest(
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    [Range(1, 22000)] int Capacity);

public record EventSessionDto(
    Guid Id,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    int Capacity,
    int AvailableTickets);

public record CreateEventRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(60)] string Genre,
    [Required] string VenueId,
    [Required, MinLength(1)] List<EventSessionRequest> Sessions);

public record ReviseEventRequest(
    [Required, StringLength(500)] string Note,
    [Required, MinLength(1)] List<EventSessionRequest> Sessions);

public record EventDto(
    Guid Id,
    string Name,
    string Genre,
    string VenueId,
    string VenueName,
    Guid ManagerId,
    string ManagerName,
    string Status,
    List<EventSessionDto> Sessions,
    List<EventSessionDto>? ProposedSessions,
    string? AdminNote);

public record BookTicketRequest(
    Guid EventId,
    Guid SessionId,
    [Range(1, 10)] int Quantity);

public record AdjustTicketRequest(
    [Range(1, 10)] int Quantity);

public record TicketDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid EventId,
    Guid SessionId,
    string EventName,
    string VenueName,
    DateTimeOffset StartsAt,
    int Quantity,
    DateTimeOffset BookedAt,
    string Status);

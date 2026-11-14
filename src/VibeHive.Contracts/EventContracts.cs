// Yevgeniy Mazur
// Event and ticket models.

using System.ComponentModel.DataAnnotations;
namespace VibeHive.Contracts;
public record RegisterRequest([Required, StringLength(80, MinimumLength=2)] string Name, [Required, EmailAddress] string Email, [Required, StringLength(128, MinimumLength=8)] string Password);
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record UserDto(Guid Id, string Name, string Email, string Role);
public record AuthResponse(string Token, DateTimeOffset ExpiresAt, UserDto User);
public record CreateEventRequest([Required, StringLength(120, MinimumLength=2)] string Title, [Required, StringLength(200)] string Venue, DateTimeOffset StartsAt, [Range(1,100000)] int Capacity);
public record EventDto(Guid Id, string Title, string Venue, DateTimeOffset StartsAt, int Capacity, int AvailableTickets, Guid OrganizerId);
public record BookTicketRequest(Guid EventId, [Range(1,10)] int Quantity);
public record TicketDto(Guid Id, Guid UserId, Guid EventId, string EventTitle, int Quantity, DateTimeOffset BookedAt, bool Cancelled);

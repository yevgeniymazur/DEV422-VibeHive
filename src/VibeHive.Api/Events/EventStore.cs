// Yevgeniy Mazur
// Event and ticket storage.

using Microsoft.AspNetCore.Identity;
using VibeHive.Contracts;
namespace VibeHive.Api.Events;
public sealed class EventStore
{
    public object Gate { get; } = new();
    public List<Account> Users { get; } = new();
    public List<EventDto> Events { get; } = new();
    public List<TicketDto> Tickets { get; } = new();
    public PasswordHasher<Account> Hasher { get; } = new();
    public EventDto WithAvailability(EventDto item) => item with { AvailableTickets = item.Capacity - Tickets.Where(t => t.EventId == item.Id && !t.Cancelled).Sum(t => t.Quantity) };
}
public sealed class Account
{
    public UserDto User { get; init; } = null!;
    public string PasswordHash { get; set; } = "";
}

// Yevgeniy Mazur
// User, event, and ticket storage.

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

    public int BookedTickets(Guid sessionId)
    {
        return Tickets
            .Where(t => t.SessionId == sessionId
                && t.Status == TicketStatuses.Booked)
            .Sum(t => t.Quantity);
    }

    public EventDto WithAvailability(EventDto item)
    {
        return item with
        {
            Sessions = item.Sessions
                .Select(session => session with
                {
                    AvailableTickets =
                        session.Capacity - BookedTickets(session.Id)
                })
                .ToList()
        };
    }

    public bool HasConflict(
        string venueId,
        IEnumerable<EventSessionRequest> sessions,
        Guid? excludeEventId = null)
    {
        var approvedSessions = Events
            .Where(e => e.VenueId == venueId
                && e.Status == EventStatuses.Approved
                && e.Id != excludeEventId)
            .SelectMany(e => e.Sessions)
            .ToList();

        return sessions.Any(requested =>
            approvedSessions.Any(existing =>
                requested.StartsAt < existing.EndsAt
                && requested.EndsAt > existing.StartsAt));
    }
}

public sealed class Account
{
    public UserDto User { get; init; } = null!;

    public string PasswordHash { get; set; } = "";
}

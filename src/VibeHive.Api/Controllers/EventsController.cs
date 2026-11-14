// Yevgeniy Mazur
// Event scheduling and approval.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(EventStore store) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpGet("venues")]
    public ActionResult<IEnumerable<VenueDto>> Venues()
    {
        return Ok(VenueCatalog.Venues);
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<EventDto>> Upcoming()
    {
        lock (store.Gate)
        {
            var events = store.Events
                .Where(e =>
                    e.Status == EventStatuses.Approved
                    && e.Sessions.Any(s =>
                        s.StartsAt > DateTimeOffset.UtcNow))
                .Select(store.WithAvailability)
                .OrderBy(e => e.Sessions.Min(s => s.StartsAt))
                .ToList();

            return Ok(events);
        }
    }

    [Authorize(Roles = UserRoles.EventManager + "," + UserRoles.Admin)]
    [HttpGet("mine")]
    public ActionResult<IEnumerable<EventDto>> Mine()
    {
        lock (store.Gate)
        {
            return Ok(store.Events
                .Where(e => e.ManagerId == CurrentUserId)
                .Select(store.WithAvailability)
                .ToList());
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("review")]
    public ActionResult<IEnumerable<EventDto>> Review()
    {
        lock (store.Gate)
        {
            return Ok(store.Events
                .Select(store.WithAvailability)
                .ToList());
        }
    }

    [Authorize(Roles = UserRoles.EventManager + "," + UserRoles.Admin)]
    [HttpPost]
    public ActionResult<EventDto> Create(CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Genre))
        {
            return BadRequest("Event name and genre are required.");
        }

        var venue = VenueCatalog.Venues
            .SingleOrDefault(v => v.Id == request.VenueId);

        if (venue is null)
            return BadRequest("Select a valid venue.");

        var error = ValidateSessions(
            request.Sessions,
            venue.MaxCapacity);

        if (error is not null)
            return BadRequest(error);

        lock (store.Gate)
        {
            var item = new EventDto(
                Guid.NewGuid(),
                request.Name.Trim(),
                request.Genre.Trim(),
                venue.Id,
                venue.Name,
                CurrentUserId,
                User.Identity?.Name ?? "Event Manager",
                EventStatuses.Pending,
                CreateSessions(request.Sessions),
                null,
                null);

            store.Events.Add(item);

            return StatusCode(201, item);
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("{id:guid}/approve")]
    public ActionResult<EventDto> Approve(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Events.FindIndex(e => e.Id == id);

            if (index < 0)
                return NotFound("Event not found.");

            var item = store.Events[index];

            if (item.Status != EventStatuses.Pending)
                return Conflict("Only pending requests can be approved.");

            var sessions = ToRequests(item.Sessions);
            var venue = VenueCatalog.Venues
                .Single(v => v.Id == item.VenueId);

            var error = ValidateSessions(
                sessions,
                venue.MaxCapacity);

            if (error is not null)
                return BadRequest(error);

            if (store.HasConflict(item.VenueId, sessions, item.Id))
            {
                return Conflict(
                    "An approved event already uses this venue during these times.");
            }

            item = item with
            {
                Status = EventStatuses.Approved,
                ProposedSessions = null
            };

            store.Events[index] = item;

            return Ok(store.WithAvailability(item));
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:guid}/proposal")]
    public ActionResult<EventDto> Propose(
        Guid id,
        ReviseEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Note))
            return BadRequest("Explain the proposed changes.");

        lock (store.Gate)
        {
            var index = store.Events.FindIndex(e => e.Id == id);

            if (index < 0)
                return NotFound("Event not found.");

            var item = store.Events[index];

            if (item.Status != EventStatuses.Pending
                && item.Status != EventStatuses.AwaitingManager)
            {
                return Conflict(
                    "Proposals apply only to requests awaiting approval.");
            }

            var venue = VenueCatalog.Venues
                .Single(v => v.Id == item.VenueId);

            var error = ValidateSessions(
                request.Sessions,
                venue.MaxCapacity);

            if (error is not null)
                return BadRequest(error);

            if (store.HasConflict(
                item.VenueId,
                request.Sessions,
                item.Id))
            {
                return Conflict(
                    "The proposed schedule overlaps an approved event.");
            }

            item = item with
            {
                Status = EventStatuses.AwaitingManager,
                ProposedSessions = CreateSessions(request.Sessions),
                AdminNote = request.Note.Trim()
            };

            store.Events[index] = item;

            return Ok(item);
        }
    }

    [Authorize(Roles = UserRoles.EventManager + "," + UserRoles.Admin)]
    [HttpPost("{id:guid}/proposal/accept")]
    public ActionResult<EventDto> AcceptProposal(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Events.FindIndex(e => e.Id == id);

            if (index < 0)
                return NotFound("Event not found.");

            var item = store.Events[index];

            if (item.ManagerId != CurrentUserId)
                return Forbid();

            if (item.Status != EventStatuses.AwaitingManager
                || item.ProposedSessions is null)
            {
                return Conflict("There is no proposal to accept.");
            }

            var sessions = ToRequests(item.ProposedSessions);
            var venue = VenueCatalog.Venues
                .Single(v => v.Id == item.VenueId);

            var error = ValidateSessions(
                sessions,
                venue.MaxCapacity);

            if (error is not null)
                return BadRequest(error);

            if (store.HasConflict(item.VenueId, sessions, item.Id))
            {
                return Conflict(
                    "The venue is no longer available for these times.");
            }

            item = item with
            {
                Status = EventStatuses.Approved,
                Sessions = item.ProposedSessions,
                ProposedSessions = null
            };

            store.Events[index] = item;

            return Ok(store.WithAvailability(item));
        }
    }

    [Authorize(Roles = UserRoles.EventManager + "," + UserRoles.Admin)]
    [HttpPost("{id:guid}/proposal/decline")]
    public ActionResult<EventDto> DeclineProposal(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Events.FindIndex(e => e.Id == id);

            if (index < 0)
                return NotFound("Event not found.");

            var item = store.Events[index];

            if (item.ManagerId != CurrentUserId)
                return Forbid();

            if (item.Status != EventStatuses.AwaitingManager)
                return Conflict("There is no proposal to decline.");

            item = item with
            {
                Status = EventStatuses.Pending,
                ProposedSessions = null,
                AdminNote = "Manager declined the proposal. Review again."
            };

            store.Events[index] = item;

            return Ok(item);
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPost("{id:guid}/reject")]
    public ActionResult<EventDto> Reject(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Events.FindIndex(e => e.Id == id);

            if (index < 0)
                return NotFound("Event not found.");

            var item = store.Events[index];

            if (item.Status != EventStatuses.Pending
                && item.Status != EventStatuses.AwaitingManager)
            {
                return Conflict(
                    "Only requests awaiting approval can be rejected.");
            }

            item = item with
            {
                Status = EventStatuses.Rejected,
                ProposedSessions = null
            };

            store.Events[index] = item;

            return Ok(item);
        }
    }

    private static string? ValidateSessions(
        List<EventSessionRequest>? sessions,
        int maxCapacity)
    {
        if (sessions is null || sessions.Count == 0)
            return "Add at least one event session.";

        if (sessions.Count > 31)
            return "An event can contain up to 31 sessions.";

        foreach (var session in sessions)
        {
            if (session.StartsAt <= DateTimeOffset.UtcNow)
                return "Sessions must start in the future.";

            if (session.EndsAt <= session.StartsAt)
                return "End time must be after start time.";

            if (session.Capacity < 1
                || session.Capacity > maxCapacity)
            {
                return $"Ticket capacity must be between 1 and {maxCapacity:N0}.";
            }
        }

        var ordered = sessions
            .OrderBy(s => s.StartsAt)
            .ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].StartsAt < ordered[i - 1].EndsAt)
                return "Sessions within the event cannot overlap.";
        }

        return null;
    }

    private static List<EventSessionDto> CreateSessions(
        IEnumerable<EventSessionRequest> sessions)
    {
        return sessions
            .OrderBy(s => s.StartsAt)
            .Select(s => new EventSessionDto(
                Guid.NewGuid(),
                s.StartsAt,
                s.EndsAt,
                s.Capacity,
                s.Capacity))
            .ToList();
    }

    private static List<EventSessionRequest> ToRequests(
        IEnumerable<EventSessionDto> sessions)
    {
        return sessions
            .Select(s => new EventSessionRequest(
                s.StartsAt,
                s.EndsAt,
                s.Capacity))
            .ToList();
    }
}

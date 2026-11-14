// Yevgeniy Mazur
// Ticket booking and management.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tickets")]
public class TicketsController(EventStore store) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    [HttpGet]
    public ActionResult<IEnumerable<TicketDto>> Mine()
    {
        lock (store.Gate)
        {
            return Ok(store.Tickets
                .Where(t => t.UserId == CurrentUserId)
                .OrderByDescending(t => t.BookedAt)
                .ToList());
        }
    }

    [HttpGet("{userId:guid}")]
    public ActionResult<IEnumerable<TicketDto>> ByUser(Guid userId)
    {
        if (userId != CurrentUserId && !IsAdmin)
            return Forbid();

        lock (store.Gate)
        {
            return Ok(store.Tickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.BookedAt)
                .ToList());
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("all")]
    public ActionResult<IEnumerable<TicketDto>> All()
    {
        lock (store.Gate)
        {
            return Ok(store.Tickets
                .OrderByDescending(t => t.BookedAt)
                .ToList());
        }
    }

    [Authorize(Roles = UserRoles.EventManager + "," + UserRoles.Admin)]
    [HttpGet("event/{eventId:guid}")]
    public ActionResult<IEnumerable<TicketDto>> ByEvent(Guid eventId)
    {
        lock (store.Gate)
        {
            var item = store.Events.SingleOrDefault(
                e => e.Id == eventId);

            if (item is null)
                return NotFound("Event not found.");

            if (item.ManagerId != CurrentUserId && !IsAdmin)
                return Forbid();

            return Ok(store.Tickets
                .Where(t => t.EventId == eventId)
                .OrderByDescending(t => t.BookedAt)
                .ToList());
        }
    }

    [HttpPost]
    public ActionResult<TicketDto> Book(BookTicketRequest request)
    {
        lock (store.Gate)
        {
            var item = store.Events.SingleOrDefault(
                e => e.Id == request.EventId);

            if (item is null)
                return NotFound("Event not found.");

            if (item.Status != EventStatuses.Approved)
                return BadRequest("This event is not open for booking.");

            var session = item.Sessions.SingleOrDefault(
                s => s.Id == request.SessionId);

            if (session is null)
                return NotFound("Event session not found.");

            if (session.StartsAt <= DateTimeOffset.UtcNow)
                return BadRequest("This session has already started.");

            var available = session.Capacity
                - store.BookedTickets(session.Id);

            if (request.Quantity > available)
                return Conflict("Not enough tickets available.");

            var ticket = new TicketDto(
                Guid.NewGuid(),
                CurrentUserId,
                User.Identity?.Name ?? "User",
                item.Id,
                session.Id,
                item.Name,
                item.VenueName,
                session.StartsAt,
                request.Quantity,
                DateTimeOffset.UtcNow,
                TicketStatuses.Booked);

            store.Tickets.Add(ticket);

            return StatusCode(201, ticket);
        }
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Cancel(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Tickets.FindIndex(t => t.Id == id);

            if (index < 0)
                return NotFound("Booking not found.");

            var ticket = store.Tickets[index];

            if (ticket.UserId != CurrentUserId && !IsAdmin)
                return Forbid();

            if (ticket.Status == TicketStatuses.Cancelled)
                return NoContent();

            if (ticket.StartsAt <= DateTimeOffset.UtcNow)
            {
                return BadRequest(
                    "Bookings cannot be cancelled after the session starts.");
            }

            store.Tickets[index] = ticket with
            {
                Status = TicketStatuses.Cancelled
            };

            return NoContent();
        }
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpPut("{id:guid}")]
    public ActionResult<TicketDto> Adjust(
        Guid id,
        AdjustTicketRequest request)
    {
        lock (store.Gate)
        {
            var index = store.Tickets.FindIndex(t => t.Id == id);

            if (index < 0)
                return NotFound("Booking not found.");

            var ticket = store.Tickets[index];

            if (ticket.Status != TicketStatuses.Booked)
                return BadRequest("Cancelled bookings cannot be adjusted.");

            if (ticket.StartsAt <= DateTimeOffset.UtcNow)
            {
                return BadRequest(
                    "Bookings cannot be adjusted after the session starts.");
            }

            var item = store.Events.SingleOrDefault(
                e => e.Id == ticket.EventId);

            if (item is null)
                return NotFound("Event not found.");

            if (item.Status != EventStatuses.Approved)
                return BadRequest("This event is not open for booking.");

            var session = item.Sessions.SingleOrDefault(
                s => s.Id == ticket.SessionId);

            if (session is null)
                return NotFound("Event session not found.");

            var availableForBooking = session.Capacity
                - store.BookedTickets(session.Id)
                + ticket.Quantity;

            if (request.Quantity > availableForBooking)
                return Conflict("Not enough tickets available.");

            ticket = ticket with
            {
                Quantity = request.Quantity
            };

            store.Tickets[index] = ticket;

            return Ok(ticket);
        }
    }
}

// Yevgeniy Mazur
// Ticket bookings and cancellation.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;
namespace VibeHive.Api.Controllers;
[ApiController, Authorize, Route("api/tickets")]
public class TicketsController(EventStore store) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet]
    public ActionResult<IEnumerable<TicketDto>> Mine()
    {
        lock (store.Gate) return Ok(store.Tickets.Where(t => t.UserId == UserId).ToArray());
    }
    [HttpPost]
    public ActionResult<TicketDto> Book(BookTicketRequest request)
    {
        lock (store.Gate)
        {
            var item = store.Events.SingleOrDefault(e => e.Id == request.EventId);
            if (item is null) return NotFound("Event not found.");
            if (item.StartsAt <= DateTimeOffset.UtcNow) return BadRequest("Event has already started.");
            if (store.WithAvailability(item).AvailableTickets < request.Quantity) return Conflict("Not enough tickets available.");
            var ticket = new TicketDto(Guid.NewGuid(), UserId, item.Id, item.Title, request.Quantity, DateTimeOffset.UtcNow, false);
            store.Tickets.Add(ticket);
            return StatusCode(201, ticket);
        }
    }
    [HttpDelete("{id:guid}")]
    public IActionResult Cancel(Guid id)
    {
        lock (store.Gate)
        {
            var index = store.Tickets.FindIndex(t => t.Id == id && t.UserId == UserId);
            if (index < 0) return NotFound();
            var ticket = store.Tickets[index];
            if (ticket.Cancelled) return NoContent();
            if (store.Events.Single(e => e.Id == ticket.EventId).StartsAt <= DateTimeOffset.UtcNow) return BadRequest("Event has already started.");
            store.Tickets[index] = ticket with { Cancelled = true };
            return NoContent();
        }
    }
}

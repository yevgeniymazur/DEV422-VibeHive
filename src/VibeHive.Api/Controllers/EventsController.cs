// Yevgeniy Mazur
// Event listings and creation.

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;
namespace VibeHive.Api.Controllers;
[ApiController, Route("api/events")]
public class EventsController(EventStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<EventDto>> List()
    {
        lock (store.Gate) return Ok(store.Events.Where(e => e.StartsAt > DateTimeOffset.UtcNow).OrderBy(e => e.StartsAt).Select(store.WithAvailability).ToArray());
    }
    [Authorize(Roles="Organizer"), HttpPost]
    public ActionResult<EventDto> Create(CreateEventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Venue)) return BadRequest("Title and venue are required.");
        if (request.StartsAt <= DateTimeOffset.UtcNow) return BadRequest("Event must start in the future.");
        var item = new EventDto(Guid.NewGuid(), request.Title.Trim(), request.Venue.Trim(), request.StartsAt, request.Capacity, request.Capacity, Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        lock (store.Gate) store.Events.Add(item);
        return StatusCode(201, item);
    }
}

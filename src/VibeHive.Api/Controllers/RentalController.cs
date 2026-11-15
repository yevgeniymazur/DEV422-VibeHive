
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Api.Music;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

// Created by Wilber Amaya-Maurisio
// Updated by Yevgeniy Mazur - 11/15/2026
// Integrated shared JWT authentication and GUID user IDs.
// Added role-based rental permissions, customer validation,
// and restricted users to managing their own rentals.
// Administrators can manage rentals for all customers.

[Authorize]
[ApiController]
[Route("api/rentals")]
public class RentalsController(EventStore store) : ControllerBase
{
    private bool IsAdmin =>
        User.IsInRole(UserRoles.Admin);

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!);

    // GET /api/rentals
    // Admin sees all active rentals.
    // Regular users only see their own rentals.
    [HttpGet]
    public ActionResult<IEnumerable<MusicRental>> GetActiveRentals()
    {
        var rentals = MusicStore.Rentals
            .Where(r => r.ReturnDate == null);

        if (!IsAdmin)
        {
            rentals = rentals.Where(
                r => r.UserId == CurrentUserId);
        }

        return Ok(rentals.ToList());
    }

    // POST /api/rentals
    // Rent an album using a registered user account.
    [HttpPost]
    public ActionResult<MusicRental> RentAlbum(MusicRental rental)
    {
        // Regular users can only rent for themselves.
        // Admins can rent on behalf of another customer.
        if (!IsAdmin || rental.UserId == Guid.Empty)
        {
            rental.UserId = CurrentUserId;
        }

        // Validate the customer and retrieve their name.
        lock (store.Gate)
        {
            var account = store.Users.FirstOrDefault(
                a => a.User.Id == rental.UserId);

            if (account == null)
                return BadRequest("Customer not found.");

            rental.UserName = account.User.Name;
        }

        var album = MusicStore.Albums.FirstOrDefault(
            a => a.Id == rental.AlbumId);

        if (album == null)
            return BadRequest(
                $"Album with id {rental.AlbumId} not found.");

        if (!album.Available)
            return BadRequest(
                "Album is not available for rent.");

        rental.Id = MusicStore.NextRentalId();
        rental.AlbumTitle = album.Title;
        rental.RentalDate = DateTime.Now;
        rental.ReturnDate = null;

        MusicStore.Rentals.Add(rental);
        album.Available = false;

        return CreatedAtAction(
            nameof(GetActiveRentals),
            new { id = rental.Id },
            rental);
    }

    // POST /api/rentals/{id}/return
    // Return an album.
    [HttpPost("{id:int}/return")]
    public IActionResult ReturnAlbum(int id)
    {
        var rental = MusicStore.Rentals.FirstOrDefault(
            r => r.Id == id);

        if (rental == null)
            return NotFound(
                $"Rental with id {id} not found.");

        // Prevent users from returning another customer's album.
        if (!IsAdmin && rental.UserId != CurrentUserId)
            return Forbid();

        if (rental.ReturnDate != null)
            return BadRequest(
                "This rental has already been returned.");

        rental.ReturnDate = DateTime.Now;

        var album = MusicStore.Albums.FirstOrDefault(
            a => a.Id == rental.AlbumId);

        if (album != null)
            album.Available = true;

        return Ok(rental);
    }
}

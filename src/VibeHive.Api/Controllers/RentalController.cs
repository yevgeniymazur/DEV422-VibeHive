using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Music;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;
// Created by Wilber Amaya-Maurisio

[ApiController]
[Route("api/rentals")]
public class RentalsController : ControllerBase
{
    // GET /api/rentals
    // Active rentals only
    [HttpGet]
    public ActionResult<IEnumerable<MusicRental>> GetActiveRentals()
    {
        return Ok(MusicStore.Rentals.Where(r => r.ReturnDate == null));
    }

    // POST /api/rentals
    // For renting an album
    [HttpPost]
    public ActionResult<MusicRental> RentAlbum(MusicRental rental)
    {
        if (rental.UserId <= 0)
            return BadRequest("A valid user id is required.");

        var album = MusicStore.Albums.FirstOrDefault(a => a.Id == rental.AlbumId);
        if (album == null)
            return BadRequest($"Album with id {rental.AlbumId} not found.");

        if (!album.Available)
            return BadRequest("Album is not available for rent.");

        rental.Id = MusicStore.NextRentalId();
        rental.UserName = string.IsNullOrWhiteSpace(rental.UserName)
            ? $"User {rental.UserId}"
            : rental.UserName.Trim();
        rental.AlbumTitle = album.Title;
        rental.RentalDate = DateTime.Now;
        rental.ReturnDate = null;

        MusicStore.Rentals.Add(rental);
        album.Available = false;

        return CreatedAtAction(nameof(GetActiveRentals), new { id = rental.Id }, rental);
    }

    // POST /api/rentals/{id}/return
    // To return an album
    [HttpPost("{id:int}/return")]
    public IActionResult ReturnAlbum(int id)
    {
        var rental = MusicStore.Rentals.FirstOrDefault(r => r.Id == id);
        if (rental == null)
            return NotFound($"Rental with id {id} not found.");

        if (rental.ReturnDate != null)
            return BadRequest("This rental has already been returned.");

        rental.ReturnDate = DateTime.Now;

        var album = MusicStore.Albums.FirstOrDefault(a => a.Id == rental.AlbumId);
        if (album != null)
            album.Available = true;

        return Ok(rental);
    }
}

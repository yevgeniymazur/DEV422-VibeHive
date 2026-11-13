using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Music;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Route("api/music")]
public class MusicController : ControllerBase
{
    // GET /api/music 
    // GET /api/music?all=true
    // Shows all albums
    public ActionResult<IEnumerable<MusicAlbum>> GetAlbums([FromQuery] bool all = false)
    {
        var albums = all ? MusicStore.Albums : MusicStore.Albums.Where(a => a.Available);
        return Ok(albums.OrderBy(a => a.Id));
    }

    // GET /api/music/{id}
    [HttpGet("{id:int}")]
    public ActionResult<MusicAlbum> GetAlbum(int id)
    {
        var album = MusicStore.Albums.FirstOrDefault(a => a.Id == id);
        if (album == null)
            return NotFound($"Album with id {id} not found.");

        return Ok(album);
    }

    // POST /api/music
    // Add a new album
    [HttpPost]
    public ActionResult<MusicAlbum> AddAlbum(MusicAlbum album)
    {
        album.Id = MusicStore.NextAlbumId();
        MusicStore.Albums.Add(album);
        return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
    }

    // DELETE /api/music/{id}
    [HttpDelete("{id:int}")]
    public IActionResult DeleteAlbum(int id)
    {
        var album = MusicStore.Albums.FirstOrDefault(a => a.Id == id);
        if (album == null)
            return NotFound($"Album with id {id} not found.");

        bool isRented = MusicStore.Rentals.Any(r => r.AlbumId == id && r.ReturnDate == null);
        if (isRented)
            return Conflict("This album is currently rented and cannot be deleted.");

        MusicStore.Albums.Remove(album);
        return NoContent();
    }
}

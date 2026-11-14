using Microsoft.AspNetCore.Mvc;

namespace VibeHive.Api.Controllers;

// Owner: TheKeybladeofSolace — collaborative playlists, songs, invitations, votes, and rankings.
[ApiController]
[Route("api/playlists")]
public class PlaylistsController : ControllerBase
{
    // TODO: Implement this module's endpoints.
    private static List<Song> songs = new();
    private static List<Playlist> playlists = new();
    private static int followingId = 1;

    [HttpPost]
    public IActionResult NewPlaylist(NewPlaylistRequest request)
    {
        var playlist = new Playlist
        {
            Id = followingId++,
            Name = request.Name,
            CreatedBy = request.CreatedBy,
            IsCollaborative = request.IsCollaborative
            Songs = new List<Song>()
        };

        playlists.Add(Playlist);

        return CreatedAction(
            nameof (GetPlaylistById)k
            new { id = Playlist.Id },
            Playlist);
    }

    [HttpGet("{id}")]
public IActionResult GetPlaylistById(int id)
{
    var playlist = playlists.FirstOrDefault(p => p.Id == id);
    if (playlist == null)
    {
        return NotFound("That playlist could not be found. Sorry.");
    }

    return Ok(playlist);
}
[HttpPut("{id}/add")]
public IActionResult AddASong(int id, AddSongRequest request)
{
    var playlist = playlists.FirstOrDefault(p => p.Id == id);

    if (playlist == null)
    {
        return NotFound("That playlist could not be found. Sorry");
    }

    var song = songs.FirstOrDefault(s => s.Id == request.SongId);
    if (song == null)
    {
        return NotFound("Sorry, we could not find that song.");
    }
    playlist.Songs.Add(song);

    return Ok(playlist);
}

[HttpPut("{id}/invite")]
public IActionResult InviteCollaborator(int id, [FromBody] InviteCollaboratorRequest request)
{
    

    if (request == null || string.IsNullOrEmpty(request.UserId))
    {
        return NotFound("That playlist could not be found. Sorry!");
    }

    var playlist = playlists.FirstOrDefault(p => p.Id == id);
    if (playlist == null)
    {
        return NotFound("Sorry, we could not find that playlist.");
    }
    if (playlist is CollaborativePlaylist collaborativePlaylist)
    {
        collaborativePlaylist.Collaborators.Add(request.UserId);
        return Ok(collaborativePlaylist);
    }
    return BadRequest("Excuse me, but only collaborative playlists can have collaborators.");
}

 [HttpPost("{id}/vote")]
 public IActionResult VoteSong(int id, VoteSongRequest request)
 {
     var playlist = playlists.FirstOrDefault(p => p.Id == id);

     if (playlist == null)
     {
         return NotFound("Sorry, we could not find that playlist. Please try again.");
     }
     if (!playlist.IsCollaborative)
     {
         return BadRequest("Voting is exclusively permitted on collaborative playlists.");
     }
     if (!playlist.Songs.Any(s => s.Id == request.SongId))
     {
         return BadRequest("Sorry, the song you are searching for ain't there. Please try again.");
     }
     var song = songs.FirstOrDefault(s => s.Id == id);
     if (song == null)
     {
         return NotFound("Sorry, the song you were looking for could not be found. Try again, perhaps?");
     }

     song.VoteIncrease();

     return Ok(song);
 }

 [HttpGet("{id}/rankings")]
public IActionResult SongRanking(int id)
{
    var playlist = playlists.FirstOrDefault(p => p.Id == id);

    if (playlist == null)
    {
        return NotFound("So sorry, that playlist must be in another place. Try again, please");
    }
    var rankedSongs = playlist.Songs
        .OrderByDescending(s => s.Votes)
        .ToList();

    return Ok(rankedSongs);
}
    
}

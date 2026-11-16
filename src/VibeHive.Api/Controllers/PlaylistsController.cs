using Microsoft.AspNetCore.Mvc;

namespace VibeHive.Api.Controllers;

// Owner: TheKeybladeofSolace — collaborative playlists, songs, invitations, votes, and rankings.
[ApiController]
[Route("api/playlists")]
public class PlaylistsController : ControllerBase
{
    // TODO: Implement this module's endpoints.

    private static List<Song> Songs = new();
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
            IsCollaborative = request.IsCollaborative,
            Songs = new List<Song>()
        };

        playlists.Add(playlist);

        return CreatedAtAction(
            nameof (GetPlaylistById),
            new { id = playlist.Id },
            playlist);
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

    var song = Songs.FirstOrDefault(s => s.Id == request.SongId);
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
     var song = Songs.FirstOrDefault(s => s.Id == id);
     if (song == null)
     {
         return NotFound("Sorry, the song you were looking for could not be found. Try again, perhaps?");
     }

     song.VoteIncrement();

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
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsCollaborative { get; set; }
        public List<Song> Songs = new();
    }
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int Votes { get; set; }
        public List<Song> Songs { get; set; } = new();

        public void VoteIncrement()
        {
            Votes++;
        }

    }
    
    public class NewPlaylistRequest
    {
        public string Name { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsCollaborative { get; set; }
    }

    public class AddSongRequest
    {
        public int SongId { get; set; }
    }

    public class InviteCollaboratorRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Collaborators { get; set; } = new();
    }

    public class VoteSongRequest
    {
        public int SongId { get; set; }
    }

    public class CollaborativePlaylist : Playlist
    {
        public List<string> Collaborators { get; set; } = new();
    }
}

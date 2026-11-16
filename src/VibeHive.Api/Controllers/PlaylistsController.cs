// Original module: Brooks Halliday
// Updated by Yevgeniy Mazur - November 15, 2026
// Connected playlists to shared JWT accounts.
// Added playlist listing, song entry, invitations,
// voting, rankings, and ownership checks.

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeHive.Api.Events;
using VibeHive.Contracts;

namespace VibeHive.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/playlists")]
public class PlaylistsController(EventStore accounts) : ControllerBase
{
    private static readonly object Gate = new();
    private static readonly List<Playlist> Playlists = new();

    private static int nextPlaylistId = 1;
    private static int nextSongId = 1;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(UserRoles.Admin);

    private bool CanView(Playlist playlist) =>
        IsAdmin ||
        playlist.OwnerId == CurrentUserId ||
        playlist.Collaborators.Any(c => c.Id == CurrentUserId);

    private bool CanManage(Playlist playlist) =>
        IsAdmin || playlist.OwnerId == CurrentUserId;

    // Owners and invited collaborators can view a playlist.
    // Admins can view every playlist.
    [HttpGet]
    public IActionResult GetPlaylists()
    {
        lock (Gate)
        {
            return Ok(Playlists
                .Where(CanView)
                .OrderBy(p => p.Name)
                .Select(Snapshot)
                .ToList());
        }
    }

    [HttpGet("{id:int}")]
    public IActionResult GetPlaylist(int id)
    {
        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanView(playlist))
                return Forbid();

            return Ok(Snapshot(playlist));
        }
    }

    [HttpPost]
    public IActionResult CreatePlaylist(CreatePlaylistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Enter a playlist name.");

        lock (Gate)
        {
            var playlist = new Playlist
            {
                Id = nextPlaylistId++,
                Name = request.Name.Trim(),
                OwnerId = CurrentUserId,
                CreatedBy = User.Identity?.Name ?? "User",
                IsCollaborative = request.IsCollaborative
            };

            Playlists.Add(playlist);

            return CreatedAtAction(
                nameof(GetPlaylist),
                new { id = playlist.Id },
                Snapshot(playlist));
        }
    }

    // The owner or an Admin can rename a playlist.
    [HttpPut("{id:int}")]
    public IActionResult RenamePlaylist(
        int id,
        RenamePlaylistRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Enter a playlist name.");

        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanManage(playlist))
                return Forbid();

            playlist.Name = request.Name.Trim();

            return Ok(Snapshot(playlist));
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeletePlaylist(int id)
    {
        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanManage(playlist))
                return Forbid();

            Playlists.Remove(playlist);

            return NoContent();
        }
    }

    // Owners, collaborators, and Admins can add songs.
    [HttpPut("{id:int}/add")]
    public IActionResult AddSong(int id, AddSongRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) ||
            string.IsNullOrWhiteSpace(request.Artist))
        {
            return BadRequest("Enter the song title and artist.");
        }

        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanView(playlist))
                return Forbid();

            var title = request.Title.Trim();
            var artist = request.Artist.Trim();

            if (playlist.Songs.Any(s =>
                s.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                s.Artist.Equals(artist, StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict("That song is already in this playlist.");
            }

            playlist.Songs.Add(new Song
            {
                Id = nextSongId++,
                Title = title,
                Artist = artist
            });

            return Ok(Snapshot(playlist));
        }
    }

    // Invite an existing registered account by email.
    [HttpPut("{id:int}/invite")]
    public IActionResult InviteCollaborator(
        int id,
        InviteCollaboratorRequest request)
    {
        UserDto? invitedUser;

        // Read the account separately to avoid nesting store locks.
        lock (accounts.Gate)
        {
            invitedUser = accounts.Users
                .Select(a => a.User)
                .FirstOrDefault(u =>
                    u.Email.Equals(
                        request.Email.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }

        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanManage(playlist))
                return Forbid();

            if (!playlist.IsCollaborative)
                return BadRequest(
                    "Only collaborative playlists allow invitations.");

            if (invitedUser is null)
                return NotFound(
                    "No registered account uses that email address.");

            if (invitedUser.Id == playlist.OwnerId)
                return BadRequest(
                    "The playlist owner already has access.");

            if (playlist.Collaborators.Any(c => c.Id == invitedUser.Id))
                return Conflict("That user is already a collaborator.");

            playlist.Collaborators.Add(new Collaborator
            {
                Id = invitedUser.Id,
                Name = invitedUser.Name,
                Email = invitedUser.Email
            });

            return Ok(Snapshot(playlist));
        }
    }

    [HttpPost("{id:int}/vote")]
    public IActionResult Vote(int id, VoteSongRequest request)
    {
        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanView(playlist))
                return Forbid();

            if (!playlist.IsCollaborative)
                return BadRequest(
                    "Voting is available on collaborative playlists.");

            // Find the selected song within this playlist.
            var song = playlist.Songs
                .FirstOrDefault(s => s.Id == request.SongId);

            if (song is null)
                return NotFound("Song not found in this playlist.");

            if (!song.Voters.Add(CurrentUserId))
                return Conflict("You already voted for this song.");

            return Ok(Snapshot(playlist));
        }
    }

    [HttpGet("{id:int}/rankings")]
    public IActionResult GetRankings(int id)
    {
        lock (Gate)
        {
            var playlist = Playlists.FirstOrDefault(p => p.Id == id);

            if (playlist is null)
                return NotFound("Playlist not found.");

            if (!CanView(playlist))
                return Forbid();

            return Ok(playlist.Songs
                .OrderByDescending(s => s.Voters.Count)
                .ThenBy(s => s.Title)
                .Select(SongSnapshot)
                .ToList());
        }
    }

    // Copy response values while holding the lock.
    // Internal voter IDs are never sent to the client.
    private object Snapshot(Playlist playlist) => new
    {
        playlist.Id,
        playlist.Name,
        playlist.OwnerId,
        playlist.CreatedBy,
        playlist.IsCollaborative,
        CanManage = CanManage(playlist),
        Songs = playlist.Songs
            .OrderByDescending(s => s.Voters.Count)
            .ThenBy(s => s.Title)
            .Select(SongSnapshot)
            .ToList(),
        Collaborators = playlist.Collaborators
            .Select(c => new { c.Id, c.Name, c.Email })
            .ToList()
    };

    private object SongSnapshot(Song song) => new
    {
        song.Id,
        song.Title,
        song.Artist,
        Votes = song.Voters.Count,
        HasVoted = song.Voters.Contains(CurrentUserId)
    };

    private sealed class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public Guid OwnerId { get; set; }
        public string CreatedBy { get; set; } = "";
        public bool IsCollaborative { get; set; }

        public List<Song> Songs { get; } = new();
        public List<Collaborator> Collaborators { get; } = new();
    }

    private sealed class Song
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";

        public HashSet<Guid> Voters { get; } = new();
    }

    private sealed class Collaborator
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public sealed class CreatePlaylistRequest
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        public bool IsCollaborative { get; set; }
    }

    public sealed class RenamePlaylistRequest
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = "";
    }

    public sealed class AddSongRequest
    {
        [Required, StringLength(160)]
        public string Title { get; set; } = "";

        [Required, StringLength(120)]
        public string Artist { get; set; } = "";
    }

    public sealed class InviteCollaboratorRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";
    }

    public sealed class VoteSongRequest
    {
        [Range(1, int.MaxValue)]
        public int SongId { get; set; }
    }
}

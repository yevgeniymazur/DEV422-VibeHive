// Original module: Brooks Halliday
// Updated by Yevgeniy Mazur - November 15, 2026
// Connected the existing playlist interface to the API and shared login.

using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeHive.Contracts;
using VibeHive.WinForms.Services;

namespace VibeHive.WinForms.Modules;

public class PlaylistsControl : UserControl
{
    private ListBox listBoxPlaylists = null!;
    private TextBox textBoxName = null!;
    private System.Windows.Forms.Timer refreshTheTimer = null!;
    private HttpClient CLient = null!;
    private ListBox listBoxOfSongs = null!;
    private TextBox textBoxMaker = null!;
    private CheckBox checkBoxCollaborator = null!;
    private Button buttonMake = null!;
    private Button buttonVoteSong = null!;
    private CheckBox chkBoxCollaborative = null!;
    private TextBox textBoxGenre = null!;
    private TextBox textBoxId = null!;
    private Button buttonDelete = null!;
    private Button buttonUpdate = null!;
    private ListBox listBoxCollaborators = null!;
    private Button buttonGet = null!;

    private readonly EventsClient sharedClient;

    private readonly TextBox textSongArtist = new();
    private readonly TextBox textInviteEmail = new();
    private readonly Button buttonAddSong = new();
    private readonly Button buttonInvite = new();
    private readonly Label labelStatus = new();

    private bool loadingPlaylists;
    private bool performingAction;

    private PlaylistData? SelectedPlaylist =>
        listBoxPlaylists.SelectedItem as PlaylistData;

    public PlaylistsControl(EventsClient client)
    {
        sharedClient = client;

        InitializeComponent();

        CLient = new HttpClient
        {
            BaseAddress = new Uri(ServiceAddresses.Api),
            Timeout = TimeSpan.FromSeconds(15)
        };

        listBoxPlaylists.DisplayMember = nameof(PlaylistData.Name);
        listBoxOfSongs.DisplayMember = nameof(SongData.Display);
        listBoxCollaborators.DisplayMember =
            nameof(CollaboratorData.Display);

        listBoxPlaylists.SelectedIndexChanged += (_, _) =>
        {
            if (!loadingPlaylists)
                DisplayPlaylist();
        };
        ForeColor = Color.White;
        chkBoxCollaborative.ForeColor = Color.White;

        foreach (var button in new[]
        {
    buttonMake,
    buttonVoteSong,
    buttonDelete,
    buttonUpdate,
    buttonGet,
    buttonAddSong,
    buttonInvite
})
        {
            button.ForeColor = Color.White;
            button.UseCompatibleTextRendering = true;
        }
        buttonMake.Click += async (_, _) =>
            await RunActionAsync(
                HttpMethod.Post,
                "api/playlists",
                new
                {
                    Name = textBoxMaker.Text.Trim(),
                    IsCollaborative = chkBoxCollaborative.Checked
                });

        buttonGet.Click += async (_, _) =>
            await LoadPlaylistsAsync();

        buttonUpdate.Click += async (_, _) =>
        {
            if (SelectedPlaylist is not { } playlist)
                return;

            await RunActionAsync(
                HttpMethod.Put,
                $"api/playlists/{playlist.Id}",
                new { Name = textBoxMaker.Text.Trim() });
        };

        buttonDelete.Click += async (_, _) =>
        {
            if (SelectedPlaylist is not { } playlist)
                return;

            if (MessageBox.Show(
                    $"Delete '{playlist.Name}'?",
                    "Delete playlist",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            await RunActionAsync(
                HttpMethod.Delete,
                $"api/playlists/{playlist.Id}");
        };

        buttonAddSong.Click += async (_, _) =>
        {
            if (SelectedPlaylist is not { } playlist)
                return;

            await RunActionAsync(
                HttpMethod.Put,
                $"api/playlists/{playlist.Id}/add",
                new
                {
                    Title = textBoxGenre.Text.Trim(),
                    Artist = textSongArtist.Text.Trim()
                });
        };

        buttonInvite.Click += async (_, _) =>
        {
            if (SelectedPlaylist is not { } playlist)
                return;

            await RunActionAsync(
                HttpMethod.Put,
                $"api/playlists/{playlist.Id}/invite",
                new { Email = textInviteEmail.Text.Trim() });
        };

        buttonVoteSong.Click += async (_, _) =>
        {
            if (SelectedPlaylist is not { } playlist ||
                listBoxOfSongs.SelectedItem is not SongData song)
            {
                return;
            }

            await RunActionAsync(
                HttpMethod.Post,
                $"api/playlists/{playlist.Id}/vote",
                new { SongId = song.Id });
        };

        refreshTheTimer = new System.Windows.Forms.Timer
        {
            Interval = 5000
        };

        refreshTheTimer.Tick += async (_, _) =>
        {
            if (Visible && !performingAction)
                await LoadPlaylistsAsync();
        };

        Load += async (_, _) =>
        {
            await RefreshAccountAsync();
            refreshTheTimer.Start();
        };

        UpdatePlaylistPermissions();
    }

    private void InitializeComponent()
    {
        buttonMake = new Button();
        buttonVoteSong = new Button();
        chkBoxCollaborative = new CheckBox();
        textBoxName = new TextBox();
        textBoxMaker = new TextBox();
        listBoxPlaylists = new ListBox();
        listBoxOfSongs = new ListBox();
        checkBoxCollaborator = new CheckBox();
        textBoxGenre = new TextBox();
        textBoxId = new TextBox();
        buttonDelete = new Button();
        buttonUpdate = new Button();
        listBoxCollaborators = new ListBox();
        buttonGet = new Button();

        SuspendLayout();

        AutoScroll = true;
        AutoScrollMinSize = new Size(960, 510);

        // Existing playlist controls.
        textBoxId.Location = new Point(10, 80);
        textBoxId.Name = "textBoxId";
        textBoxId.Size = new Size(190, 27);
        textBoxId.ReadOnly = true;
        textBoxId.PlaceholderText = "Selected playlist ID";

        textBoxMaker.Location = new Point(10, 115);
        textBoxMaker.Name = "textBoxMaker";
        textBoxMaker.Size = new Size(190, 27);
        textBoxMaker.PlaceholderText = "Playlist name";

        textBoxName.Location = new Point(10, 150);
        textBoxName.Name = "textBoxName";
        textBoxName.Size = new Size(190, 27);
        textBoxName.ReadOnly = true;
        textBoxName.PlaceholderText = "Signed-in user";

        // Reuse the existing genre field for song entry.
        textBoxGenre.Location = new Point(10, 200);
        textBoxGenre.Name = "textBoxGenre";
        textBoxGenre.Size = new Size(190, 27);
        textBoxGenre.PlaceholderText = "Song title";

        textSongArtist.Location = new Point(10, 235);
        textSongArtist.Name = "textSongArtist";
        textSongArtist.Size = new Size(190, 27);
        textSongArtist.PlaceholderText = "Song artist";

        buttonAddSong.Location = new Point(10, 275);
        buttonAddSong.Name = "buttonAddSong";
        buttonAddSong.Size = new Size(190, 32);
        buttonAddSong.Text = "Add song";

        chkBoxCollaborative.Location = new Point(10, 320);
        chkBoxCollaborative.Name = "chkBoxCollaborative";
        chkBoxCollaborative.AutoSize = true;
        chkBoxCollaborative.Text = "Collaborative";
        chkBoxCollaborative.Checked = true;

        buttonMake.Location = new Point(10, 355);
        buttonMake.Name = "buttonMake";
        buttonMake.Size = new Size(190, 36);
        buttonMake.Text = "Create Playlist";

        buttonUpdate.Location = new Point(10, 405);
        buttonUpdate.Name = "buttonUpdate";
        buttonUpdate.Size = new Size(90, 32);
        buttonUpdate.Text = "Rename";

        buttonDelete.Location = new Point(110, 405);
        buttonDelete.Name = "buttonDelete";
        buttonDelete.Size = new Size(90, 32);
        buttonDelete.Text = "Delete";

        // Existing playlist, song, and collaborator lists.
        listBoxPlaylists.Location = new Point(219, 70);
        listBoxPlaylists.Name = "listBoxPlaylists";
        listBoxPlaylists.Size = new Size(250, 280);
        listBoxPlaylists.HorizontalScrollbar = true;

        buttonGet.Location = new Point(219, 365);
        buttonGet.Name = "buttonGet";
        buttonGet.Size = new Size(120, 32);
        buttonGet.Text = "Refresh";

        listBoxOfSongs.Location = new Point(485, 70);
        listBoxOfSongs.Name = "listBoxOfSongs";
        listBoxOfSongs.Size = new Size(207, 280);
        listBoxOfSongs.HorizontalScrollbar = true;

        buttonVoteSong.Location = new Point(485, 365);
        buttonVoteSong.Name = "buttonVoteSong";
        buttonVoteSong.Size = new Size(207, 36);
        buttonVoteSong.Text = "Vote for selected song";

        listBoxCollaborators.Location = new Point(707, 70);
        listBoxCollaborators.Name = "listBoxCollaborators";
        listBoxCollaborators.Size = new Size(227, 280);
        listBoxCollaborators.HorizontalScrollbar = true;

        // Preserve the existing unused checkbox.
        checkBoxCollaborator.Name = "checkBoxCollaborator";
        checkBoxCollaborator.Text = "Collaborator";
        checkBoxCollaborator.Visible = false;

        textInviteEmail.Location = new Point(707, 365);
        textInviteEmail.Name = "textInviteEmail";
        textInviteEmail.Size = new Size(227, 27);
        textInviteEmail.PlaceholderText = "Registered user's email";

        buttonInvite.Location = new Point(707, 405);
        buttonInvite.Name = "buttonInvite";
        buttonInvite.Size = new Size(227, 32);
        buttonInvite.Text = "Invite user";

        labelStatus.Location = new Point(10, 465);
        labelStatus.Name = "labelStatus";
        labelStatus.Size = new Size(925, 45);
        labelStatus.Text = "Sign in to use playlists.";

        var labelPlaylists = new Label
        {
            Text = "Playlists",
            AutoSize = true,
            Location = new Point(219, 40)
        };

        var labelSongs = new Label
        {
            Text = "Songs and rankings",
            AutoSize = true,
            Location = new Point(485, 40)
        };

        var labelCollaborators = new Label
        {
            Text = "Collaborators",
            AutoSize = true,
            Location = new Point(707, 40)
        };

        Controls.AddRange(new Control[]
        {
            textBoxId,
            textBoxMaker,
            textBoxName,
            textBoxGenre,
            textSongArtist,
            buttonAddSong,
            chkBoxCollaborative,
            buttonMake,
            buttonUpdate,
            buttonDelete,
            listBoxPlaylists,
            buttonGet,
            listBoxOfSongs,
            buttonVoteSong,
            listBoxCollaborators,
            checkBoxCollaborator,
            textInviteEmail,
            buttonInvite,
            labelStatus,
            labelPlaylists,
            labelSongs,
            labelCollaborators
        });

        Name = "PlaylistsControl";
        Size = new Size(1066, 540);

        ResumeLayout(false);
        PerformLayout();
    }

    public async Task RefreshAccountAsync()
    {
        listBoxPlaylists.DataSource = null;
        listBoxOfSongs.DataSource = null;
        listBoxCollaborators.DataSource = null;

        textBoxId.Clear();
        textBoxMaker.Clear();
        textBoxGenre.Clear();
        textSongArtist.Clear();
        textInviteEmail.Clear();

        textBoxName.Text = sharedClient.Session?.User.Name ?? "";

        UpdatePlaylistPermissions();
        await LoadPlaylistsAsync();
    }

    private async Task LoadPlaylistsAsync()
    {
        if (loadingPlaylists || IsDisposed ||
            sharedClient.Session is null)
        {
            return;
        }

        var session = sharedClient.Session;
        var selectedId = SelectedPlaylist?.Id;
        var selectedSongId =
            (listBoxOfSongs.SelectedItem as SongData)?.Id;

        loadingPlaylists = true;

        try
        {
            using var request = MakeRequest(
                HttpMethod.Get,
                "api/playlists");

            using var response = await CLient.SendAsync(request);
            await CheckResponseAsync(response);

            var playlists = await response.Content
                .ReadFromJsonAsync<List<PlaylistData>>() ?? new();

            if (!ReferenceEquals(session, sharedClient.Session) ||
                IsDisposed)
            {
                return;
            }

            listBoxPlaylists.DataSource = playlists;

            var previous = playlists.FirstOrDefault(
                p => p.Id == selectedId);

            if (previous is not null)
                listBoxPlaylists.SelectedItem = previous;

            DisplayPlaylist();

            if (selectedSongId.HasValue &&
                SelectedPlaylist is { } selected)
            {
                var song = selected.Songs.FirstOrDefault(
                    s => s.Id == selectedSongId.Value);

                if (song is not null)
                    listBoxOfSongs.SelectedItem = song;
            }

            labelStatus.Text = playlists.Count == 0
                ? "Create a playlist or ask an owner to invite you."
                : "Songs ranked by votes. Refreshes every 5 seconds.";
        }
        catch (Exception ex)
        {
            if (ReferenceEquals(session, sharedClient.Session) &&
                !IsDisposed)
            {
                labelStatus.Text = ex.Message;
            }
        }
        finally
        {
            loadingPlaylists = false;

            if (!IsDisposed &&
                !ReferenceEquals(session, sharedClient.Session))
            {
                await LoadPlaylistsAsync();
            }
        }
    }

    private void DisplayPlaylist()
    {
        var playlist = SelectedPlaylist;

        textBoxId.Text = playlist?.Id.ToString() ?? "";

        listBoxOfSongs.DataSource = playlist?.Songs;
        listBoxCollaborators.DataSource = playlist?.Collaborators;

        UpdatePlaylistPermissions();
    }

    private void UpdatePlaylistPermissions()
    {
        var signedIn = sharedClient.Session is not null;
        var enabled = signedIn && !performingAction;
        var playlist = SelectedPlaylist;

        textBoxName.Text = sharedClient.Session?.User.Name ?? "";

        listBoxPlaylists.Enabled = enabled;

        textBoxMaker.Enabled = enabled;
        chkBoxCollaborative.Enabled = enabled;

        buttonMake.Enabled = enabled;
        buttonGet.Enabled = enabled;

        buttonUpdate.Enabled =
            enabled && playlist?.CanManage == true;

        buttonDelete.Enabled =
            enabled && playlist?.CanManage == true;

        buttonAddSong.Enabled =
            enabled && playlist is not null;

        textBoxGenre.Enabled = buttonAddSong.Enabled;
        textSongArtist.Enabled = buttonAddSong.Enabled;

        buttonVoteSong.Enabled =
            enabled && playlist?.IsCollaborative == true;

        buttonInvite.Enabled =
            enabled &&
            playlist is not null &&
            playlist.CanManage &&
            playlist.IsCollaborative;

        textInviteEmail.Enabled = buttonInvite.Enabled;

        if (!signedIn)
            labelStatus.Text = "Sign in to use playlists.";
    }

    private HttpRequestMessage MakeRequest(
        HttpMethod method,
        string path)
    {
        var request = new HttpRequestMessage(method, path);

        if (sharedClient.Session is { } session)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    session.Token);
        }

        return request;
    }

    private async Task RunActionAsync(
        HttpMethod method,
        string path,
        object? body = null)
    {
        if (performingAction || sharedClient.Session is null)
            return;

        performingAction = true;
        UpdatePlaylistPermissions();

        try
        {
            using var request = MakeRequest(method, path);

            if (body is not null)
                request.Content = JsonContent.Create(body);

            using var response = await CLient.SendAsync(request);
            await CheckResponseAsync(response);

            await LoadPlaylistsAsync();
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                MessageBox.Show(
                    ex.Message,
                    "Playlists",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        finally
        {
            performingAction = false;

            if (!IsDisposed)
                UpdatePlaylistPermissions();
        }
    }

    private static async Task CheckResponseAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "Please sign out and sign in again.");
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                "Your account does not have permission for this action.");
        }

        var message = await response.Content.ReadAsStringAsync();

        throw new InvalidOperationException(
            string.IsNullOrWhiteSpace(message)
                ? $"Request failed ({(int)response.StatusCode})."
                : message);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            refreshTheTimer?.Stop();
            refreshTheTimer?.Dispose();
            CLient?.Dispose();
        }

        base.Dispose(disposing);
    }

    public sealed class PlaylistData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public bool IsCollaborative { get; set; }
        public bool CanManage { get; set; }

        public List<SongData> Songs { get; set; } = new();
        public List<CollaboratorData> Collaborators { get; set; } = new();
    }

    public sealed class SongData
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public int Votes { get; set; }
        public bool HasVoted { get; set; }

        public string Display =>
            $"{Title} - {Artist} ({Votes} votes)" +
            (HasVoted ? " [voted]" : "");
    }

    public sealed class CollaboratorData
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";

        public string Display => $"{Name} ({Email})";
    }
}

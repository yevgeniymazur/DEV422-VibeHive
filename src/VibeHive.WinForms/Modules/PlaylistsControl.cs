namespace VibeHive.WinForms.Modules;

// Owner: TheKeybladeofSolace — Playlist creation, songs, collaborators, voting, and rankings.
public class PlaylistsControl : UserControl
{
    public PlaylistsControl()
    {
        Controls.Add(new Label
        {
            Text = "Collaborative Playlists — module placeholder\nOwner: TheKeybladeofSolace\nPlaylist creation, songs, collaborators, voting, and rankings.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        });
        // TODO: Build this module's screen and connect its service client.
    }
}

namespace VibeHive.WinForms.Modules;

// Owner: TheKeybladeofSolace — Playlist creation, songs, collaborators, voting, and rankings.
public class PlaylistsControl : UserControl
{
    private ListBox listBoxPlaylists;
    private TextBox textBoxName;
    private System.Windows.Forms.Timer refreshTheTimer;
    private HttpClient CLient;
    private ListBox listBoxOfSongs;
    private TextBox textBoxMaker;
    private CheckBox checkBoxCollaborator;
    private Button buttonMake;
    private Button buttonVoteSong;
    
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

    private void InitializeComponentLayout()
    {
        this.SuspendLayout();

        listBoxPlaylists = new ListBox();
        listBoxPlaylists.Location = new Point(10, 50);
        listBoxPlaylists.Size = new Size(250, 200);

        listBoxOfSongs = new ListBox();
        listBoxOfSongs.Location = new Point(260, 50);
        listBoxOfSongs.Size = new Size(250, 150);

        textBoxName = new TextBox();
        textBoxName.Location = new Point(10, 270);
        textBoxName.Width = 190;

        textBoxMaker = new TextBox();
        textBoxMaker.Location = new Point(10, 260);
        textBoxMaker.Width = 190;

        checkBoxCollaborator = new CheckBox();
        checkBoxCollaborator.Location = new Point(10, 320);
        checkBoxCollaborator.Text = "Collaborative";

        buttonMake = new Button();
        buttonMake.Location = new Point(10, 310);
        buttonMake.Size = new Size(120, 40);
        buttonMake.Text = "Make Playlist";

        buttonVoteSong = new Button();
        buttonVoteSong.Location = new Point(150, 360);
        buttonVoteSong.Size = new Size(120, 40);
        buttonVoteSong.Text = "Vote Song";

        Controls.Add(listBoxPlaylists);
        Controls.Add(listBoxOfSongs);
        Controls.Add(textBoxName);
        Controls.Add(textBoxMaker);
        Controls.Add(checkBoxCollaborator);
        Controls.Add(buttonMake);
        Controls.Add(buttonVoteSong);

        this.ResumeLayout();
    }
}

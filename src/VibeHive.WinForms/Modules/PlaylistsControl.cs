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
    private TextBox textBoxGenre;
    private TextBox textBoxId;
    private Button buttonDelete;
    private Button buttonUpdate;
    private ListBox listBoxCollaborators;
    private Button buttonGet;
    private CheckBox chkBoxCollaborative;


    public PlaylistsControl()
    {
        InitializeComponent();

        Controls.Add(new Label
        {
            Text = "Collaborative Playlists",
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
        // 
        // buttonMake
        // 
        buttonMake.Location = new Point(10, 350);
        buttonMake.Name = "buttonMake";
        buttonMake.Size = new Size(120, 40);
        buttonMake.TabIndex = 0;
        buttonMake.Text = "Create Playlist";
        buttonMake.UseVisualStyleBackColor = true;
        // 
        // buttonVoteSong
        // 
        buttonVoteSong.Location = new Point(150, 360);
        buttonVoteSong.Name = "buttonVoteSong";
        buttonVoteSong.Size = new Size(120, 40);
        buttonVoteSong.TabIndex = 1;
        buttonVoteSong.Text = "Vote Song";
        buttonVoteSong.UseVisualStyleBackColor = true;
        // 
        // chkBoxCollaborative
        // 
        chkBoxCollaborative.AutoSize = true;
        chkBoxCollaborative.Location = new Point(10, 320);
        chkBoxCollaborative.Name = "chkBoxCollaborative";
        chkBoxCollaborative.Size = new Size(120, 24);
        chkBoxCollaborative.TabIndex = 3;
        chkBoxCollaborative.Text = "Collaborative";
        chkBoxCollaborative.UseVisualStyleBackColor = true;
        // 
        // textBoxName
        // 
        textBoxName.Location = new Point(10, 145);
        textBoxName.Name = "textBoxName";
        textBoxName.Size = new Size(198, 27);
        textBoxName.TabIndex = 4;
        textBoxName.Text = "Created By:";
        // 
        // textBoxMaker
        // 
        textBoxMaker.Location = new Point(10, 110);
        textBoxMaker.Name = "textBoxMaker";
        textBoxMaker.Size = new Size(190, 27);
        textBoxMaker.TabIndex = 5;
        textBoxMaker.Text = "Playlist Name:";
        // 
        // listBoxPlaylists
        // 
        listBoxPlaylists.FormattingEnabled = true;
        listBoxPlaylists.Location = new Point(219, 70);
        listBoxPlaylists.Name = "listBoxPlaylists";
        listBoxPlaylists.Size = new Size(250, 184);
        listBoxPlaylists.TabIndex = 6;
        // 
        // listBoxOfSongs
        // 
        listBoxOfSongs.FormattingEnabled = true;
        listBoxOfSongs.Location = new Point(475, 50);
        listBoxOfSongs.Name = "listBoxOfSongs";
        listBoxOfSongs.Size = new Size(207, 304);
        listBoxOfSongs.TabIndex = 7;
        // 
        // checkBoxCollaborator
        // 
        checkBoxCollaborator.AutoSize = true;
        checkBoxCollaborator.Location = new Point(343, 383);
        checkBoxCollaborator.Name = "checkBoxCollaborator";
        checkBoxCollaborator.Size = new Size(115, 24);
        checkBoxCollaborator.TabIndex = 8;
        checkBoxCollaborator.Text = "Collaborator";
        checkBoxCollaborator.UseVisualStyleBackColor = true;
        // 
        // textBoxGenre
        // 
        textBoxGenre.Location = new Point(10, 175);
        textBoxGenre.Name = "textBoxGenre";
        textBoxGenre.Size = new Size(138, 27);
        textBoxGenre.TabIndex = 9;
        textBoxGenre.Text = "Genre:";
        // 
        // textBoxId
        // 
        textBoxId.Location = new Point(10, 80);
        textBoxId.Name = "textBoxId";
        textBoxId.Size = new Size(181, 27);
        textBoxId.TabIndex = 10;
        textBoxId.Text = "Playlist ID:";
        // 
        // buttonDelete
        // 
        buttonDelete.Location = new Point(283, 316);
        buttonDelete.Name = "buttonDelete";
        buttonDelete.Size = new Size(136, 32);
        buttonDelete.TabIndex = 11;
        buttonDelete.Text = "Delete";
        buttonDelete.UseVisualStyleBackColor = true;
        // 
        // buttonUpdate
        // 
        buttonUpdate.Location = new Point(469, 418);
        buttonUpdate.Name = "buttonUpdate";
        buttonUpdate.Size = new Size(127, 33);
        buttonUpdate.TabIndex = 12;
        buttonUpdate.Text = "Update";
        buttonUpdate.UseVisualStyleBackColor = true;
        // 
        // listBoxCollaborators
        // 
        listBoxCollaborators.FormattingEnabled = true;
        listBoxCollaborators.Location = new Point(707, 51);
        listBoxCollaborators.Name = "listBoxCollaborators";
        listBoxCollaborators.Size = new Size(227, 304);
        listBoxCollaborators.TabIndex = 13;
        // 
        // buttonGet
        // 
        buttonGet.Location = new Point(666, 410);
        buttonGet.Name = "buttonGet";
        buttonGet.Size = new Size(166, 32);
        buttonGet.TabIndex = 14;
        buttonGet.Text = "Get";
        buttonGet.UseVisualStyleBackColor = true;
        // 
        // PlaylistsControl
        // 
        Controls.Add(buttonGet);
        Controls.Add(listBoxCollaborators);
        Controls.Add(checkBoxCollaborator);
        Controls.Add(listBoxOfSongs);
        Controls.Add(listBoxPlaylists);
        Controls.Add(textBoxMaker);
        Controls.Add(textBoxName);
        Controls.Add(chkBoxCollaborative);
        Controls.Add(buttonVoteSong);
        Controls.Add(buttonMake);
        Controls.Add(buttonDelete);
        Controls.Add(buttonUpdate);
        Controls.Add(textBoxId);
        Controls.Add(textBoxGenre);
        Name = "PlaylistsControl";
        Size = new Size(1066, 514);
        ResumeLayout(false);
        PerformLayout();

        //
        // CheckBoxCollaborator
        //

    }

}

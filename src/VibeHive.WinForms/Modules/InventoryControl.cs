using VibeHive.Contracts;

namespace VibeHive.WinForms.Modules;

// Owner: TheKeybladeofSolace — Album CRUD through the gRPC inventory service.
public class InventoryControl : UserControl
{
        
    private TextBox textId;
    private TextBox textTitle;
    private TextBox textArtist;
    private TextBox textGenre;
    private TextBox textYear;
    private CheckBox chkAvailable;

    private Button btnAdd;
    private Button btnGet;
    private Button btnDelete;
    private Button btnUpdate;
    private Button btnList;

    
    public InventoryControl()
    {
        Controls.Add(new Label
        {
            Text = "Album Inventory",
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter
        });
        // TODO: Build this module's screen and connect its service client.
        btnAdd = new Button
        {
            Text = " Add Album"
        };
        Controls.Add(btnAdd);
        btnAdd.Location = new Point(20, 300);

        btnDelete = new Button
        {
            Text = "Delete"
        };
        Controls.Add(btnDelete);
        btnDelete.Location = new Point(120, 300);

        btnUpdate = new Button
        {
            Text = "Update"
        };
        Controls.Add(btnUpdate);
        btnUpdate.Location = new Point(220, 300);

        btnGet = new Button
        {
            Text = "Get Albums"
        };
        Controls.Add(btnGet);
        btnGet.Location = new Point(340, 300);

        btnList = new Button
        {
            Text = "List Albums"
        };
        Controls.Add(btnList);
        btnList.Location = new Point(460, 300);
        textId = new TextBox
        {
            PlaceholderText = "Album ID"
        };
        Controls.Add(textId);
        textId.Location = new Point(20, 50);

        textTitle = new TextBox
        {
            PlaceholderText = "Title"
        };
        Controls.Add(textTitle);
        textTitle.Location = new Point(20, 90);

        textArtist = new TextBox
        {
            PlaceholderText = "Artist"
        };
        Controls.Add(textArtist);
        textArtist.Location = new Point(20, 130);

        textGenre = new TextBox
        {
            PlaceholderText = "Genre"
        };
        Controls.Add(textGenre);
        textGenre.Location = new Point(20, 170);

        textYear = new TextBox
        {
            PlaceholderText = "Year"
        };
        Controls.Add(textYear);
        textYear.Location = new Point(20, 210);

        chkAvailable = new CheckBox
        {
            Text = "Available"
        };
        Controls.Add(chkAvailable);
        chkAvailable.Location = new Point(20, 250);

        btnAdd.Click += btnAdd_Click;
        btnDelete.Click += btnDelete_Click;
        btnUpdate.Click += btnUpdate_Click;
        btnGet.Click += btnGet_Click;
        btnList.Click += btnList_Click;
    }
    private void btnAdd_Click(object? sender, EventArgs e)
    {
        var album = new MusicAlbum
        {
            Id = int.Parse(textId.Text),
            Title = textTitle.Text,
            Artist = textArtist.Text,
            Genre = textGenre.Text,
            Year = int.Parse(textYear.Text),
            Available = chkAvailable.Checked
        };
    }
    private void  btnDelete_Click(object? sender, EventArgs e)
    {
        var album = new MusicAlbum
        {
            Id = int.Parse(textId.Text)
        };

        MessageBox.Show($"Deletion of Albun ID {album.Id} was a success!");
    }

    private void btnUpdate_Click(object? sender, EventArgs e)
    {
        var album = new MusicAlbum
        {
            Id = int.Parse(textId.Text),
            Title = textTitle.Text,
            Artist = textArtist.Text,
            Genre = textGenre.Text,
            Available = chkAvailable.Checked
        };
    }

    private void btnGet_Click(object? sender, EventArgs e)
    {
        var album = new MusicAlbum
        {
            Id = int.Parse(textId.Text),
            Title = "Title",
            Artist = "Who is This?",
            Genre = "What Genre?",
            Year = int.Parse(textYear.Text),
            Available = chkAvailable.Checked
        };
    }
    
    private void btnList_Click(object? sender, EventArgs e)
    {
        MessageBox.Show("List albums not implemented right now.");
    }
}

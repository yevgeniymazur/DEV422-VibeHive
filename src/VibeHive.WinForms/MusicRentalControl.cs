using System.ComponentModel;
using System.Net.Http.Json;
using VibeHive.Contracts;

namespace VibeHive.WinForms.Modules;
// Created by Wilber Amaya-Maurisio
// Music Rental screen
[DesignerCategory("Code")]
public class MusicRentalControl : UserControl
{
    private static readonly HttpClient Client = new() { BaseAddress = new Uri(ServiceAddresses.Api) };

    private readonly SplitContainer _split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };

    // Album section
    private readonly TextBox _txtTitle = new() { Width = 140 };
    private readonly TextBox _txtArtist = new() { Width = 140 };
    private readonly TextBox _txtGenre = new() { Width = 100 };
    private readonly TextBox _txtYear = new() { Width = 60 };
    private readonly CheckBox _chkAvailable = new() { Text = "Available", Checked = true, AutoSize = true };
    private readonly Button _btnAddAlbum = new() { Text = "Add Album", AutoSize = true };
    private readonly Button _btnDeleteAlbum = new() { Text = "Delete Selected Album", AutoSize = true };
    private readonly Button _btnRefreshAlbums = new() { Text = "Refresh", AutoSize = true };
    private readonly DataGridView _gridAlbums = CreateGrid();

    // Rental section
    private readonly TextBox _txtUserId = new() { Width = 60 };
    private readonly TextBox _txtUserName = new() { Width = 140 };
    private readonly Button _btnRent = new() { Text = "Rent Selected Album", AutoSize = true };
    private readonly Button _btnReturn = new() { Text = "Return Selected Rental", AutoSize = true };
    private readonly Button _btnRefreshRentals = new() { Text = "Refresh", AutoSize = true };
    private readonly DataGridView _gridRentals = CreateGrid();

    public MusicRentalControl()
    {
        _split.Panel1.Controls.Add(BuildAlbumSection());
        _split.Panel2.Controls.Add(BuildRentalSection());
        Controls.Add(_split);

        _btnAddAlbum.Click += async (_, _) => await AddAlbumAsync();
        _btnDeleteAlbum.Click += async (_, _) => await DeleteAlbumAsync();
        _btnRefreshAlbums.Click += async (_, _) => await LoadAlbumsAsync();
        _btnRent.Click += async (_, _) => await RentAlbumAsync();
        _btnReturn.Click += async (_, _) => await ReturnAlbumAsync();
        _btnRefreshRentals.Click += async (_, _) => await LoadRentalsAsync();

        Load += async (_, _) =>
        {
            if (DesignMode) return;
            if (_split.Height > 200)
                _split.SplitterDistance = _split.Height / 2;
            await RefreshAllAsync();
        };
    }

    // Lets the shared app fill in the logged in user from the login module and locks the fields
    public void SetCurrentUser(int userId, string userName)
    {
        _txtUserId.Text = userId.ToString();
        _txtUserName.Text = userName;
        _txtUserId.ReadOnly = true;
        _txtUserName.ReadOnly = true;
    }

    // Layout

    private Control BuildAlbumSection()
    {
        var inputs = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        inputs.Controls.AddRange(new Control[]
        {
            MakeLabel("Title:"), _txtTitle,
            MakeLabel("Artist:"), _txtArtist,
            MakeLabel("Genre:"), _txtGenre,
            MakeLabel("Year:"), _txtYear,
            _chkAvailable, _btnAddAlbum
        });

        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange(new Control[] { _btnDeleteAlbum, _btnRefreshAlbums });

        return BuildSection("Music Albums", inputs, _gridAlbums, buttons);
    }

    private Control BuildRentalSection()
    {
        var inputs = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        inputs.Controls.AddRange(new Control[]
        {
            MakeLabel("User ID:"), _txtUserId,
            MakeLabel("User name:"), _txtUserName,
            _btnRent
        });

        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange(new Control[] { _btnReturn, _btnRefreshRentals });

        return BuildSection("Active Rentals", inputs, _gridRentals, buttons);
    }

    private static TableLayoutPanel BuildSection(string title, Control inputs, Control grid, Control buttons)
    {
        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Margin = new Padding(3, 6, 3, 6)
        };

        table.Controls.Add(header, 0, 0);
        table.Controls.Add(inputs, 0, 1);
        table.Controls.Add(grid, 0, 2);
        table.Controls.Add(buttons, 0, 3);
        return table;
    }

    private static Label MakeLabel(string text) =>
        new() { Text = text, AutoSize = true, Margin = new Padding(8, 6, 2, 0) };

    private static DataGridView CreateGrid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false
    };

    // Loading

    private async Task RefreshAllAsync()
    {
        await LoadAlbumsAsync();
        await LoadRentalsAsync();
    }

    private async Task LoadAlbumsAsync()
    {
        try
        {
            var albums = await Client.GetFromJsonAsync<List<MusicAlbum>>("api/music?all=true");
            _gridAlbums.DataSource = albums;
        }
        catch (Exception ex)
        {
            ShowError("Could not load albums", ex);
        }
    }

    private async Task LoadRentalsAsync()
    {
        try
        {
            var rentals = await Client.GetFromJsonAsync<List<MusicRental>>("api/rentals");
            _gridRentals.DataSource = rentals;
        }
        catch (Exception ex)
        {
            ShowError("Could not load rentals", ex);
        }
    }

    // Albums

    private async Task AddAlbumAsync()
    {
        if (string.IsNullOrWhiteSpace(_txtTitle.Text) || string.IsNullOrWhiteSpace(_txtArtist.Text))
        {
            MessageBox.Show("Please enter both a title and an artist.");
            return;
        }

        if (!int.TryParse(_txtYear.Text, out int year) || year < 1900 || year > 2100)
        {
            MessageBox.Show("Please enter a valid year between 1900 and 2100.");
            return;
        }

        var album = new MusicAlbum
        {
            Title = _txtTitle.Text.Trim(),
            Artist = _txtArtist.Text.Trim(),
            Genre = _txtGenre.Text.Trim(),
            Year = year,
            Available = _chkAvailable.Checked
        };

        try
        {
            var response = await Client.PostAsJsonAsync("api/music", album);
            if (response.IsSuccessStatusCode)
            {
                _txtTitle.Clear();
                _txtArtist.Clear();
                _txtGenre.Clear();
                _txtYear.Clear();
                _chkAvailable.Checked = true;
                await LoadAlbumsAsync();
            }
            else
            {
                MessageBox.Show(await DescribeErrorAsync(response, "Could not add the album"));
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not add the album", ex);
        }
    }

    private async Task DeleteAlbumAsync()
    {
        if (_gridAlbums.CurrentRow?.DataBoundItem is not MusicAlbum album)
        {
            MessageBox.Show("Select an album to delete first.");
            return;
        }

        try
        {
            var response = await Client.DeleteAsync($"api/music/{album.Id}");
            if (response.IsSuccessStatusCode)
                await LoadAlbumsAsync();
            else
                MessageBox.Show(await DescribeErrorAsync(response, "Could not delete the album"));
        }
        catch (Exception ex)
        {
            ShowError("Could not delete the album", ex);
        }
    }

    // Rentals

    private async Task RentAlbumAsync()
    {
        if (_gridAlbums.CurrentRow?.DataBoundItem is not MusicAlbum album)
        {
            MessageBox.Show("Select an album in the albums list first.");
            return;
        }

        if (!album.Available)
        {
            MessageBox.Show("That album is already rented.");
            return;
        }

        if (!int.TryParse(_txtUserId.Text, out int userId) || userId <= 0)
        {
            MessageBox.Show("Please enter a valid user ID.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtUserName.Text))
        {
            MessageBox.Show("Please enter the user's name.");
            return;
        }

        var rental = new MusicRental
        {
            UserId = userId,
            UserName = _txtUserName.Text.Trim(),
            AlbumId = album.Id
        };

        try
        {
            var response = await Client.PostAsJsonAsync("api/rentals", rental);
            if (response.IsSuccessStatusCode)
                await RefreshAllAsync();
            else
                MessageBox.Show(await DescribeErrorAsync(response, "Could not rent the album"));
        }
        catch (Exception ex)
        {
            ShowError("Could not rent the album", ex);
        }
    }

    private async Task ReturnAlbumAsync()
    {
        if (_gridRentals.CurrentRow?.DataBoundItem is not MusicRental rental)
        {
            MessageBox.Show("Select a rental to return first.");
            return;
        }

        try
        {
            var response = await Client.PostAsync($"api/rentals/{rental.Id}/return", null);
            if (response.IsSuccessStatusCode)
                await RefreshAllAsync();
            else
                MessageBox.Show(await DescribeErrorAsync(response, "Could not return the album"));
        }
        catch (Exception ex)
        {
            ShowError("Could not return the album", ex);
        }
    }

    // Helpers

    private static async Task<string> DescribeErrorAsync(HttpResponseMessage response, string prefix)
    {
        string body = await response.Content.ReadAsStringAsync();
        return string.IsNullOrWhiteSpace(body)
            ? $"{prefix} ({(int)response.StatusCode})."
            : $"{prefix}: {body}";
    }

    private static void ShowError(string message, Exception ex)
    {
        MessageBox.Show($"{message}: {ex.Message}\n\nMake sure VibeHive.Api is running.");
    }
}


using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VibeHive.Contracts;
using VibeHive.WinForms.Services;

namespace VibeHive.WinForms.Modules;

// Created by Wilber Amaya-Maurisio
// Updated by Yevgeniy Mazur - 11/15/2026
// Integrated shared JWT authentication and GUID user accounts.
// Added role-based permissions for album and rental management.
// Added customer selection for administrators.
// Regular users automatically rent under their own accounts.
// Guests can browse albums without signing in.

[DesignerCategory("Code")]
public class MusicRentalControl : UserControl
{
    private readonly EventsClient _auth;

    private readonly HttpClient Client = new()
    {
        BaseAddress = new Uri(ServiceAddresses.Api)
    };

    private readonly SplitContainer _split = new()
    {
        Dock = DockStyle.Fill,
        Orientation = Orientation.Horizontal
    };

    // Album section
    private readonly TextBox _txtTitle = new() { Width = 140 };
    private readonly TextBox _txtArtist = new() { Width = 140 };
    private readonly TextBox _txtGenre = new() { Width = 100 };
    private readonly TextBox _txtYear = new() { Width = 60 };

    private readonly CheckBox _chkAvailable = new()
    {
        Text = "Available",
        Checked = true,
        AutoSize = true
    };

    private readonly Button _btnAddAlbum = new()
    {
        Text = "Add Album",
        AutoSize = true
    };

    private readonly Button _btnDeleteAlbum = new()
    {
        Text = "Delete Selected Album",
        AutoSize = true
    };

    private readonly Button _btnRefreshAlbums = new()
    {
        Text = "Refresh",
        AutoSize = true
    };

    private readonly DataGridView _gridAlbums = CreateGrid();

    // Rental section
    private readonly Label _lblCustomer = MakeLabel("Customer:");

    private readonly ComboBox _cmbCustomers = new()
    {
        Width = 260,
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly Label _lblCurrentUser = MakeLabel(
        "Sign in to rent albums.");

    private readonly Button _btnRent = new()
    {
        Text = "Rent Selected Album",
        AutoSize = true
    };

    private readonly Button _btnReturn = new()
    {
        Text = "Return Selected Rental",
        AutoSize = true
    };

    private readonly Button _btnRefreshRentals = new()
    {
        Text = "Refresh",
        AutoSize = true
    };

    private readonly DataGridView _gridRentals = CreateGrid();

    // Shared authentication
    private bool IsSignedIn =>
        _auth.Session is not null;

    private bool IsAdmin =>
        _auth.Session?.User.Role == UserRoles.Admin;

    public MusicRentalControl(EventsClient sharedClient)
    {
        _auth = sharedClient;

        _split.Panel1.Controls.Add(BuildAlbumSection());
        _split.Panel2.Controls.Add(BuildRentalSection());

        Controls.Add(_split);

        _btnAddAlbum.Click += async (_, _) =>
            await AddAlbumAsync();

        _btnDeleteAlbum.Click += async (_, _) =>
            await DeleteAlbumAsync();

        _btnRefreshAlbums.Click += async (_, _) =>
            await LoadAlbumsAsync();

        _btnRent.Click += async (_, _) =>
            await RentAlbumAsync();

        _btnReturn.Click += async (_, _) =>
            await ReturnAlbumAsync();

        _btnRefreshRentals.Click += async (_, _) =>
            await LoadRentalsAsync();

        Load += async (_, _) =>
        {
            if (DesignMode)
                return;

            if (_split.Height > 200)
                _split.SplitterDistance = _split.Height / 2;

            await RefreshAccountAsync();
        };

        UpdatePermissions();
    }

    // Refresh the module when the shared account changes.
    public async Task RefreshAccountAsync()
    {
        var session = _auth.Session;

        Client.DefaultRequestHeaders.Authorization =
            session is null
                ? null
                : new AuthenticationHeaderValue(
                    "Bearer", session.Token);

        // Clear data from the previous account.
        _gridRentals.DataSource = null;
        _cmbCustomers.DataSource = null;

        UpdatePermissions();

        if (IsAdmin)
            await LoadCustomersAsync();

        await RefreshAllAsync();
    }

    // Apply permissions based on the current account role.
    private void UpdatePermissions()
    {
        _txtTitle.Enabled = IsAdmin;
        _txtArtist.Enabled = IsAdmin;
        _txtGenre.Enabled = IsAdmin;
        _txtYear.Enabled = IsAdmin;
        _chkAvailable.Enabled = IsAdmin;

        _btnAddAlbum.Enabled = IsAdmin;
        _btnDeleteAlbum.Enabled = IsAdmin;

        _btnRent.Enabled = IsSignedIn;
        _btnReturn.Enabled = IsSignedIn;
        _btnRefreshRentals.Enabled = IsSignedIn;

        _lblCustomer.Visible = IsAdmin;
        _cmbCustomers.Visible = IsAdmin;
        _lblCurrentUser.Visible = !IsAdmin;

        var user = _auth.Session?.User;

        _lblCurrentUser.Text = user is null
            ? "Sign in to rent albums."
            : $"Customer: {user.Name}";
    }

    // Load registered customers for administrators.
    private async Task LoadCustomersAsync()
    {
        if (!IsAdmin)
            return;

        try
        {
            var customers =
                await Client.GetFromJsonAsync<List<UserDto>>(
                    "api/users");

            _cmbCustomers.DisplayMember = nameof(UserDto.Name);
            _cmbCustomers.ValueMember = nameof(UserDto.Id);

            _cmbCustomers.DataSource =
                customers ?? new List<UserDto>();

            var user = _auth.Session?.User;

            if (user is not null)
                _cmbCustomers.SelectedValue = user.Id;
        }
        catch (Exception ex)
        {
            ShowError("Could not load customers", ex);
        }
    }

    // Layout
    private Control BuildAlbumSection()
    {
        var inputs = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        inputs.Controls.AddRange(new Control[]
        {
            MakeLabel("Title:"), _txtTitle,
            MakeLabel("Artist:"), _txtArtist,
            MakeLabel("Genre:"), _txtGenre,
            MakeLabel("Year:"), _txtYear,
            _chkAvailable,
            _btnAddAlbum
        });

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        buttons.Controls.AddRange(new Control[]
        {
            _btnDeleteAlbum,
            _btnRefreshAlbums
        });

        return BuildSection(
            "Music Albums",
            inputs,
            _gridAlbums,
            buttons);
    }

    private Control BuildRentalSection()
    {
        var inputs = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        inputs.Controls.AddRange(new Control[]
        {
            _lblCustomer,
            _cmbCustomers,
            _lblCurrentUser,
            _btnRent
        });

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill
        };

        buttons.Controls.AddRange(new Control[]
        {
            _btnReturn,
            _btnRefreshRentals
        });

        return BuildSection(
            "Active Rentals",
            inputs,
            _gridRentals,
            buttons);
    }

    private static TableLayoutPanel BuildSection(
        string title,
        Control inputs,
        Control grid,
        Control buttons)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };

        table.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        table.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        table.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        table.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        table.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        var header = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font(
                SystemFonts.DefaultFont,
                FontStyle.Bold),
            Margin = new Padding(3, 6, 3, 6)
        };

        table.Controls.Add(header, 0, 0);
        table.Controls.Add(inputs, 0, 1);
        table.Controls.Add(grid, 0, 2);
        table.Controls.Add(buttons, 0, 3);

        return table;
    }

    private static Label MakeLabel(string text) =>
        new()
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(8, 6, 2, 0)
        };

    private static DataGridView CreateGrid() =>
        new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            MultiSelect = false,
            SelectionMode =
                DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill,
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
            var albums =
                await Client.GetFromJsonAsync<List<MusicAlbum>>(
                    "api/music?all=true");

            _gridAlbums.DataSource = albums;
        }
        catch (Exception ex)
        {
            ShowError("Could not load albums", ex);
        }
    }

    private async Task LoadRentalsAsync()
    {
        if (!IsSignedIn)
        {
            _gridRentals.DataSource = null;
            return;
        }

        var session = _auth.Session;

        try
        {
            var rentals =
                await Client.GetFromJsonAsync<List<MusicRental>>(
                    "api/rentals");

            // Do not display results from a previous account.
            if (!ReferenceEquals(session, _auth.Session))
                return;

            _gridRentals.DataSource = rentals;

            // Hide technical identifiers from the rental grid.
            if (_gridRentals.Columns.Contains("UserId"))
                _gridRentals.Columns["UserId"]!.Visible = false;

            if (_gridRentals.Columns.Contains("AlbumId"))
                _gridRentals.Columns["AlbumId"]!.Visible = false;

            if (_gridRentals.Columns.Contains("UserName"))
                _gridRentals.Columns["UserName"]!.HeaderText =
                    "Customer";

            if (_gridRentals.Columns.Contains("AlbumTitle"))
                _gridRentals.Columns["AlbumTitle"]!.HeaderText =
                    "Album";

            if (_gridRentals.Columns.Contains("RentalDate"))
                _gridRentals.Columns["RentalDate"]!.HeaderText =
                    "Rental Date";

            if (_gridRentals.Columns.Contains("ReturnDate"))
                _gridRentals.Columns["ReturnDate"]!.HeaderText =
                    "Return Date";
        }
        catch (Exception ex)
        {
            if (ReferenceEquals(session, _auth.Session))
                ShowError("Could not load rentals", ex);
        }
    }

    // Album management
    private async Task AddAlbumAsync()
    {
        if (!IsAdmin)
        {
            MessageBox.Show(
                "Only administrators can add albums.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtTitle.Text) ||
            string.IsNullOrWhiteSpace(_txtArtist.Text))
        {
            MessageBox.Show(
                "Please enter both a title and an artist.");
            return;
        }

        if (!int.TryParse(_txtYear.Text, out int year) ||
            year < 1900 || year > 2100)
        {
            MessageBox.Show(
                "Please enter a valid year between 1900 and 2100.");
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
            var response = await Client.PostAsJsonAsync(
                "api/music", album);

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
                MessageBox.Show(
                    await DescribeErrorAsync(
                        response,
                        "Could not add the album"));
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not add the album", ex);
        }
    }

    private async Task DeleteAlbumAsync()
    {
        if (!IsAdmin)
        {
            MessageBox.Show(
                "Only administrators can delete albums.");
            return;
        }

        if (_gridAlbums.CurrentRow?.DataBoundItem
            is not MusicAlbum album)
        {
            MessageBox.Show(
                "Select an album to delete first.");
            return;
        }

        try
        {
            var response = await Client.DeleteAsync(
                $"api/music/{album.Id}");

            if (response.IsSuccessStatusCode)
            {
                await LoadAlbumsAsync();
            }
            else
            {
                MessageBox.Show(
                    await DescribeErrorAsync(
                        response,
                        "Could not delete the album"));
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not delete the album", ex);
        }
    }

    // Rentals
    private async Task RentAlbumAsync()
    {
        var user = _auth.Session?.User;

        if (user is null)
        {
            MessageBox.Show(
                "Please sign in before renting an album.");
            return;
        }

        if (_gridAlbums.CurrentRow?.DataBoundItem
            is not MusicAlbum album)
        {
            MessageBox.Show(
                "Select an album in the albums list first.");
            return;
        }

        if (!album.Available)
        {
            MessageBox.Show(
                "That album is already rented.");
            return;
        }

        // Regular users rent under their own account.
        var customer = user;

        // Administrators can select another customer.
        if (IsAdmin)
        {
            if (_cmbCustomers.SelectedItem
                is not UserDto selectedCustomer)
            {
                MessageBox.Show(
                    "Please select a customer.");
                return;
            }

            customer = selectedCustomer;
        }

        var rental = new MusicRental
        {
            UserId = customer.Id,
            UserName = customer.Name,
            AlbumId = album.Id
        };

        try
        {
            var response = await Client.PostAsJsonAsync(
                "api/rentals", rental);

            if (response.IsSuccessStatusCode)
            {
                await RefreshAllAsync();
            }
            else
            {
                MessageBox.Show(
                    await DescribeErrorAsync(
                        response,
                        "Could not rent the album"));
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not rent the album", ex);
        }
    }

    private async Task ReturnAlbumAsync()
    {
        var user = _auth.Session?.User;

        if (user is null)
        {
            MessageBox.Show(
                "Please sign in before returning an album.");
            return;
        }

        if (_gridRentals.CurrentRow?.DataBoundItem
            is not MusicRental rental)
        {
            MessageBox.Show(
                "Select a rental to return first.");
            return;
        }

        if (!IsAdmin && rental.UserId != user.Id)
        {
            MessageBox.Show(
                "You can only return your own rentals.");
            return;
        }

        try
        {
            var response = await Client.PostAsync(
                $"api/rentals/{rental.Id}/return",
                null);

            if (response.IsSuccessStatusCode)
            {
                await RefreshAllAsync();
            }
            else
            {
                MessageBox.Show(
                    await DescribeErrorAsync(
                        response,
                        "Could not return the album"));
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not return the album", ex);
        }
    }

    // Helpers
    private static async Task<string> DescribeErrorAsync(
        HttpResponseMessage response,
        string prefix)
    {
        string body =
            await response.Content.ReadAsStringAsync();

        return string.IsNullOrWhiteSpace(body)
            ? $"{prefix} ({(int)response.StatusCode})."
            : $"{prefix}: {body}";
    }

    private static void ShowError(
        string message,
        Exception ex)
    {
        MessageBox.Show(
            $"{message}: {ex.Message}\n\n" +
            "Make sure VibeHive.Api is running.");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Client.Dispose();

        base.Dispose(disposing);
    }
}


using VibeHive.WinForms.Modules;
using VibeHive.WinForms.Services;
using VibeHive.WinForms.UI;

namespace VibeHive.WinForms;

// Yevgeniy Mazur - 11/15/2026
// Shared application shell and authentication.
// Integrated Events & Tickets and Music Rental with a shared JWT session.
// Added consistent dark styling and custom navigation tabs.
// Account changes automatically refresh connected modules.

public class MainForm : Form
{
    private readonly EventsClient client = new();

    private readonly EventsTicketsControl eventsControl;
    private readonly MusicRentalControl musicRental;

    private readonly Label accountLabel =
        DarkTheme.Label("Browsing as guest");

    private readonly Button loginButton =
        DarkTheme.Button("Sign in / Register");

    private readonly Button logoutButton =
        DarkTheme.Button("Sign out");

    public MainForm()
    {
        Text = "VibeHive";
        Width = 1100;
        Height = 800;
        MinimumSize = new Size(950, 650);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };

        layout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 65));

        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100));

        // Application header
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12, 5, 12, 5)
        };

        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        header.ColumnStyles.Add(
            new ColumnStyle(SizeType.AutoSize));

        var heading = DarkTheme.Label("VibeHive", true);
        heading.Anchor = AnchorStyles.Left;

        var accountBar = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        accountBar.Controls.Add(accountLabel);
        accountBar.Controls.Add(loginButton);
        accountBar.Controls.Add(logoutButton);

        header.Controls.Add(heading, 0, 0);
        header.Controls.Add(accountBar, 1, 0);

        // Main navigation tabs
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(190, 42),
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };

        tabs.DrawItem += DrawTab;

        tabs.SelectedIndexChanged += (_, _) =>
            tabs.Invalidate();

        // Events and Tickets - shared authentication
        eventsControl = new EventsTicketsControl(client);

        AddTab(tabs, "Events & Tickets", eventsControl);

        // Collaborative Playlists
        var playlists = new PlaylistsControl();

        DarkTheme.Apply(playlists);

        AddTab(tabs, "Playlists", playlists);

        // Music Rental - shared authentication
        musicRental = new MusicRentalControl(client);

        DarkTheme.Apply(musicRental);

        AddTab(tabs, "Music Rental", musicRental);

        // Album Inventory is not included in this submission.
        // AddTab(tabs, "Album Inventory", new InventoryControl());

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(tabs, 0, 1);

        Controls.Add(layout);

        // Application theme
        DarkTheme.Apply(header);

        BackColor = DarkTheme.Background;
        layout.BackColor = DarkTheme.Background;

        heading.Font = new Font(
            "Segoe UI", 24, FontStyle.Bold);

        // Shared login
        loginButton.Click += async (_, _) =>
        {
            using var dialog = new AccountDialog(client);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await RefreshModulesAsync();
            }
        };

        // Shared logout
        logoutButton.Click += async (_, _) =>
        {
            client.Logout();

            await RefreshModulesAsync();
        };

        UpdateAccount();
    }

    // Refresh connected modules after login or logout.
    private async Task RefreshModulesAsync()
    {
        UpdateAccount();

        loginButton.Enabled = false;
        logoutButton.Enabled = false;

        try
        {
            await Task.WhenAll(
                musicRental.RefreshAccountAsync(),
                eventsControl.RefreshAccountAsync());
        }
        finally
        {
            loginButton.Enabled = true;
            logoutButton.Enabled = true;
        }
    }

    // Update the shared account header.
    private void UpdateAccount()
    {
        var user = client.Session?.User;

        accountLabel.Text = user is null
            ? "Browsing as guest"
            : $"{user.Name} • {user.Role}";

        loginButton.Visible = user is null;
        logoutButton.Visible = user is not null;
    }

    // Custom dark navigation tabs.
    private static void DrawTab(
        object? sender,
        DrawItemEventArgs e)
    {
        if (sender is not TabControl tabs)
            return;

        if (e.Index < 0 || e.Index >= tabs.TabPages.Count)
            return;

        var selected = tabs.SelectedIndex == e.Index;
        var bounds = e.Bounds;

        using var background = new SolidBrush(
            selected
                ? DarkTheme.Accent
                : DarkTheme.Input);

        e.Graphics.FillRectangle(background, bounds);

        TextRenderer.DrawText(
            e.Graphics,
            tabs.TabPages[e.Index].Text,
            tabs.Font,
            bounds,
            selected
                ? Color.White
                : DarkTheme.Muted,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.SingleLine);
    }

    // Add a module to the application.
    private static void AddTab(
        TabControl tabs,
        string title,
        UserControl content)
    {
        var page = new TabPage(title)
        {
            BackColor = DarkTheme.Background,
            UseVisualStyleBackColor = false
        };

        content.Dock = DockStyle.Fill;

        page.Controls.Add(content);
        tabs.TabPages.Add(page);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            client.Dispose();

        base.Dispose(disposing);
    }
}

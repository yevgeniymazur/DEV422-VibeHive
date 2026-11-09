using VibeHive.WinForms.Modules;

namespace VibeHive.WinForms;

// Shared application shell. Keep module screens in Modules/ to reduce merge conflicts.
public class MainForm : Form
{
    public MainForm()
    {
        Text = "VibeHive";
        Width = 1100;
        Height = 750;
        StartPosition = FormStartPosition.CenterScreen;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        AddTab(tabs, "Events & Tickets", new EventsTicketsControl());
        AddTab(tabs, "Playlists", new PlaylistsControl());
        AddTab(tabs, "Album Inventory", new InventoryControl());
        Controls.Add(tabs);
    }

    private static void AddTab(TabControl tabs, string title, UserControl content)
    {
        var page = new TabPage(title);
        content.Dock = DockStyle.Fill;
        page.Controls.Add(content);
        tabs.TabPages.Add(page);
    }
}

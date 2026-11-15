// Yevgeniy Mazur
// Events and tickets screen.

using VibeHive.Contracts;
using VibeHive.WinForms.Services;
using VibeHive.WinForms.UI;

namespace VibeHive.WinForms.Modules;

public partial class EventsTicketsControl : UserControl
{
    private readonly EventsClient client = new();

    private readonly Label accountLabel =
        DarkTheme.Label("Browsing as guest");

    private readonly Label statusLabel =
        DarkTheme.Label("Sign in to book tickets.");

    private readonly Label scheduleNote =
        DarkTheme.Label("Select an event to view its sessions.");

    private readonly DataGridView eventsGrid = DarkTheme.Grid();
    private readonly DataGridView sessionsGrid = DarkTheme.Grid();
    private readonly DataGridView ticketsGrid = DarkTheme.Grid();

    private readonly ComboBox eventView = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 190
    };

    private readonly NumericUpDown quantity = new()
    {
        Minimum = 1,
        Maximum = 10,
        Value = 1,
        Width = 65
    };

    private readonly CheckBox showProposal = new()
    {
        Text = "Show proposed schedule",
        AutoSize = true
    };

    private readonly Button accountButton =
        DarkTheme.Button("Sign in / Register");

    private readonly Button logoutButton =
        DarkTheme.Button("Sign out");

    private readonly Button createButton =
        DarkTheme.Button("Request event");

    private readonly Button approveButton =
        DarkTheme.Button("Approve");

    private readonly Button proposeButton =
        DarkTheme.Button("Propose changes");

    private readonly Button rejectButton =
        DarkTheme.Button("Reject");

    private readonly Button acceptButton =
        DarkTheme.Button("Accept proposal");

    private readonly Button declineButton =
        DarkTheme.Button("Decline proposal");

    private readonly Button bookButton =
        DarkTheme.Button("Book tickets");

    private readonly Button cancelButton =
        DarkTheme.Button("Cancel booking");

    private readonly Button adjustButton =
        DarkTheme.Button("Adjust quantity");

    private readonly Button myTicketsButton =
        DarkTheme.Button("My bookings");

    private readonly Button eventTicketsButton =
        DarkTheme.Button("Selected event bookings");

    private readonly Button allTicketsButton =
        DarkTheme.Button("All bookings");

    private readonly TabControl pages = new()
    {
        Dock = DockStyle.Fill
    };

    private bool busy;
    private bool updatingView;

    public EventsTicketsControl()
    {
        Dock = DockStyle.Fill;
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = DarkTheme.Label("Events & Tickets", true);

        var accountBar = Bar(
            accountLabel,
            accountButton,
            logoutButton);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(accountBar, 0, 1);
        layout.Controls.Add(pages, 0, 2);
        layout.Controls.Add(statusLabel, 0, 3);

        BuildEventsPage();
        BuildBookingsPage();

        Controls.Add(layout);

        DarkTheme.Apply(this);

        BackColor = DarkTheme.Background;
        layout.BackColor = DarkTheme.Background;
        heading.Font = new Font("Segoe UI", 19, FontStyle.Bold);

        ConfigureColumns();
        ConnectActions();
        UpdateAccount();

        Load += async (_, _) => await Run(RefreshEvents);
    }

    private void BuildEventsPage()
    {
        var page = new TabPage("Events");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var refresh = DarkTheme.Button("Refresh");
        refresh.Click += async (_, _) => await Run(RefreshEvents);

        layout.Controls.Add(
            Bar(eventView, refresh, createButton), 0, 0);

        layout.Controls.Add(eventsGrid, 0, 1);

        layout.Controls.Add(
            Bar(
                approveButton,
                proposeButton,
                rejectButton,
                acceptButton,
                declineButton), 0, 2);

        scheduleNote.MaximumSize = new Size(850, 0);

        layout.Controls.Add(
            Bar(showProposal, scheduleNote), 0, 3);

        layout.Controls.Add(sessionsGrid, 0, 4);

        layout.Controls.Add(
            Bar(
                DarkTheme.Label("Tickets"),
                quantity,
                bookButton), 0, 5);

        page.Controls.Add(layout);
        pages.TabPages.Add(page);
    }

    private void BuildBookingsPage()
    {
        var page = new TabPage("Bookings");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(
            Bar(
                myTicketsButton,
                eventTicketsButton,
                allTicketsButton), 0, 0);

        layout.Controls.Add(ticketsGrid, 0, 1);

        layout.Controls.Add(
            Bar(cancelButton, adjustButton), 0, 2);

        page.Controls.Add(layout);
        pages.TabPages.Add(page);
    }

    private void ConfigureColumns()
    {
        AddColumn(eventsGrid, "Event", nameof(EventDto.Name));
        AddColumn(eventsGrid, "Venue", nameof(EventDto.VenueName));
        AddColumn(eventsGrid, "Genre", nameof(EventDto.Genre));
        AddColumn(eventsGrid, "Manager", nameof(EventDto.ManagerName));
        AddColumn(eventsGrid, "Status", nameof(EventDto.Status));

        AddColumn(
            sessionsGrid, "Start",
            nameof(EventSessionDto.StartsAt), "g");

        AddColumn(
            sessionsGrid, "End",
            nameof(EventSessionDto.EndsAt), "g");

        AddColumn(
            sessionsGrid, "Capacity",
            nameof(EventSessionDto.Capacity), "N0");

        AddColumn(
            sessionsGrid, "Available",
            nameof(EventSessionDto.AvailableTickets), "N0");

        AddColumn(ticketsGrid, "Event", nameof(TicketDto.EventName));
        AddColumn(ticketsGrid, "Venue", nameof(TicketDto.VenueName));
        AddColumn(ticketsGrid, "Name", nameof(TicketDto.UserName));

        AddColumn(
            ticketsGrid, "Start",
            nameof(TicketDto.StartsAt), "g");

        AddColumn(ticketsGrid, "Tickets", nameof(TicketDto.Quantity));
        AddColumn(ticketsGrid, "Status", nameof(TicketDto.Status));
    }

    private static void AddColumn(
        DataGridView grid,
        string title,
        string property,
        string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            HeaderText = title,
            DataPropertyName = property,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };

        if (format is not null)
            column.DefaultCellStyle.Format = format;

        grid.Columns.Add(column);
    }

    private static FlowLayoutPanel Bar(params Control[] controls)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(2, 4, 2, 4)
        };

        panel.Controls.AddRange(controls);

        return panel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            client.Dispose();

        base.Dispose(disposing);
    }
}

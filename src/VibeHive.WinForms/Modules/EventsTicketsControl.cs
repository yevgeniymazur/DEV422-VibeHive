// Yevgeniy Mazur
// Events and tickets screen.

using VibeHive.Contracts;
using VibeHive.WinForms.Services;
namespace VibeHive.WinForms.Modules;

public class EventsTicketsControl : UserControl
{
    private readonly EventsClient client = new();
    private readonly TextBox name = new() { PlaceholderText = "Name (registration)", Width = 155 };
    private readonly TextBox email = new() { PlaceholderText = "Email", Width = 190 };
    private readonly TextBox password = new() { PlaceholderText = "Password", UseSystemPasswordChar = true, Width = 150 };
    private readonly Label status = new() { AutoSize = true, Text = "Browse events or sign in to book tickets.", Padding = new Padding(5) };
    private readonly DataGridView events = Grid();
    private readonly DataGridView tickets = Grid();
    private readonly NumericUpDown quantity = new() { Minimum = 1, Maximum = 10, Value = 1, Width = 55 };
    private readonly TextBox title = new() { PlaceholderText = "Event title", Width = 170 };
    private readonly TextBox venue = new() { PlaceholderText = "Venue", Width = 170 };
    private readonly DateTimePicker starts = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm", Width = 155, Value = DateTime.Now.AddDays(1) };
    private readonly NumericUpDown capacity = new() { Minimum = 1, Maximum = 100000, Value = 100, Width = 75 };
    private readonly FlowLayoutPanel organizer = new() { Dock = DockStyle.Fill, AutoSize = true, Visible = false };
    private readonly Button login;
    private readonly Button register;
    private readonly Button logout;
    private readonly Button book;
    private readonly Button cancel;
    private bool busy;

    public EventsTicketsControl()
    {
        Dock = DockStyle.Fill;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8, Padding = new Padding(10) };
        for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(i is 3 or 6 ? SizeType.Percent : SizeType.AutoSize, i is 3 or 6 ? 50 : 0));
        var auth = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        register = ActionButton("Register", async () => {
            await client.Register(new(name.Text, email.Text, password.Text));
            password.Clear(); status.Text = "Registered. Enter your password and sign in.";
        });
        login = ActionButton("Sign in", async () => {
            await client.Login(new(email.Text, password.Text)); password.Clear(); UpdateSession(); await RefreshData();
        });
        logout = ActionButton("Sign out", () => { client.Logout(); tickets.DataSource = null; UpdateSession(); return Task.CompletedTask; });
        auth.Controls.AddRange([name, email, password, register, login, logout]);
        layout.Controls.Add(auth, 0, 0);
        layout.Controls.Add(status, 0, 1);
        var eventActions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        book = ActionButton("Book selected event", async () => {
            if (events.CurrentRow?.DataBoundItem is not EventDto item) throw new InvalidOperationException("Select an event first.");
            await client.Book(new(item.Id, (int)quantity.Value)); await RefreshData(); status.Text = "Tickets booked.";
        });
        eventActions.Controls.AddRange([ActionButton("Refresh events", RefreshData), new Label { Text = "Quantity", AutoSize = true, Padding = new Padding(0, 7, 0, 0) }, quantity, book]);
        layout.Controls.Add(eventActions, 0, 2);
        layout.Controls.Add(events, 0, 3);
        organizer.Controls.AddRange([title, venue, starts, capacity, ActionButton("Create event", async () => {
            await client.Create(new(title.Text, venue.Text, new DateTimeOffset(starts.Value), (int)capacity.Value));
            title.Clear(); venue.Clear(); await RefreshData(); status.Text = "Event created.";
        })]);
        layout.Controls.Add(organizer, 0, 4);
        layout.Controls.Add(new Label { Text = "My ticket bookings", AutoSize = true }, 0, 5);
        layout.Controls.Add(tickets, 0, 6);
        cancel = ActionButton("Cancel selected booking", async () => {
            if (tickets.CurrentRow?.DataBoundItem is not TicketDto ticket) throw new InvalidOperationException("Select a booking first.");
            await client.Cancel(ticket.Id); await RefreshData(); status.Text = "Booking cancelled.";
        });
        layout.Controls.Add(cancel, 0, 7);
        Controls.Add(layout);
        UpdateSession();
        Load += async (_, _) => await Run(RefreshData);
    }
    private static DataGridView Grid() => new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
    private Button ActionButton(string text, Func<Task> action)
    {
        var button = new Button { Text = text, AutoSize = true };
        button.Click += async (_, _) => await Run(action);
        return button;
    }
    private async Task Run(Func<Task> action)
    {
        if (busy) return;
        busy = true; Enabled = false;
        try { await action(); }
        catch (Exception ex) { status.Text = ex.Message; }
        finally { busy = false; Enabled = true; }
    }
    private void UpdateSession()
    {
        var user = client.Session?.User;
        login.Enabled = register.Enabled = user is null;
        logout.Enabled = book.Enabled = cancel.Enabled = user is not null;
        organizer.Visible = user?.Role == "Organizer";
        status.Text = user is null ? "Browse events or sign in to book tickets." : $"Signed in: {user.Name} ({user.Role})";
    }
    private async Task RefreshData()
    {
        events.DataSource = await client.Events();
        tickets.DataSource = client.Session is null ? null : await client.Tickets();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) client.Dispose();
        base.Dispose(disposing);
    }
}

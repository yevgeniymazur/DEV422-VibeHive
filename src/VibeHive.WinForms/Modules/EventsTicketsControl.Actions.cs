// Yevgeniy Mazur
// Event screen actions.

using VibeHive.Contracts;
using VibeHive.WinForms.UI;

namespace VibeHive.WinForms.Modules;

public partial class EventsTicketsControl
{
    private string bookingView = "mine";
    private Guid? bookingEventId;

    private EventDto? SelectedEvent =>
        eventsGrid.CurrentRow?.DataBoundItem as EventDto;

    private EventSessionDto? SelectedSession =>
        sessionsGrid.CurrentRow?.DataBoundItem as EventSessionDto;

    private TicketDto? SelectedTicket =>
        ticketsGrid.CurrentRow?.DataBoundItem as TicketDto;

    private bool IsAdmin =>
        client.Session?.User.Role == UserRoles.Admin;

    private bool IsManager =>
        client.Session?.User.Role == UserRoles.EventManager;

    private void ConnectActions()
    {
        accountButton.Click += async (_, _) => await Run(async () =>
        {
            using var dialog = new AccountDialog(client);

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                return;

            bookingView = "mine";
            bookingEventId = null;

            UpdateAccount();
            await RefreshEvents();
            await RefreshBookings();

            statusLabel.Text = "Signed in successfully.";
        });

        logoutButton.Click += async (_, _) => await Run(async () =>
        {
            client.Logout();
            bookingView = "mine";
            bookingEventId = null;
            ticketsGrid.DataSource = null;

            UpdateAccount();
            await RefreshEvents();

            statusLabel.Text = "Signed out.";
        });

        eventView.SelectedIndexChanged += async (_, _) =>
        {
            if (!updatingView)
                await Run(RefreshEvents);
        };

        eventsGrid.SelectionChanged += (_, _) => ShowSessions();
        sessionsGrid.SelectionChanged += (_, _) => UpdateButtons();
        ticketsGrid.SelectionChanged += (_, _) => UpdateButtons();
        showProposal.CheckedChanged += (_, _) => ShowSessions();
        quantity.ValueChanged += (_, _) => UpdateButtons();

        createButton.Click += async (_, _) => await Run(async () =>
        {
            using var dialog = new ScheduleDialog();

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK
                || dialog.NewEvent is null)
            {
                return;
            }

            await Change(
                () => client.Create(dialog.NewEvent),
                "Event request submitted for admin review.");
        });

        approveButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            await Change(
                () => client.Approve(item.Id),
                "Event approved and open for booking.");
        });

        rejectButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            if (!Confirm($"Reject the request for {item.Name}?"))
                return;

            await Change(
                () => client.Reject(item.Id),
                "Event request rejected.");
        });

        proposeButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            using var dialog = new ScheduleDialog(item);

            if (dialog.ShowDialog(FindForm()) != DialogResult.OK
                || dialog.Proposal is null)
            {
                return;
            }

            await Change(
                () => client.Propose(item.Id, dialog.Proposal),
                "Proposal sent to the event manager.");
        });

        acceptButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            if (!Confirm("Accept the proposed dates, times and capacities?"))
                return;

            await Change(
                () => client.AcceptProposal(item.Id),
                "Proposal accepted. Event approved.");
        });

        declineButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            await Change(
                () => client.DeclineProposal(item.Id),
                "Proposal declined. Request returned to admin review.");
        });

        bookButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            var session = SelectedSession
                ?? throw new InvalidOperationException(
                    "Select an event session.");

            await Change(
                () => client.Book(new BookTicketRequest(
                    item.Id,
                    session.Id,
                    (int)quantity.Value)),
                "Tickets booked successfully.");
        });

        myTicketsButton.Click += async (_, _) => await Run(async () =>
        {
            bookingView = "mine";
            bookingEventId = null;

            await RefreshBookings();
        });

        allTicketsButton.Click += async (_, _) => await Run(async () =>
        {
            bookingView = "all";
            bookingEventId = null;

            await RefreshBookings();
        });

        eventTicketsButton.Click += async (_, _) => await Run(async () =>
        {
            var item = RequireEvent();

            bookingView = "event";
            bookingEventId = item.Id;

            await RefreshBookings();
        });

        cancelButton.Click += async (_, _) => await Run(async () =>
        {
            var ticket = SelectedTicket
                ?? throw new InvalidOperationException(
                    "Select a booking.");

            if (!Confirm($"Cancel the booking for {ticket.EventName}?"))
                return;

            await Change(
                () => client.Cancel(ticket.Id),
                "Booking cancelled. Tickets returned to availability.");
        });

        adjustButton.Click += async (_, _) => await Run(async () =>
        {
            var ticket = SelectedTicket
                ?? throw new InvalidOperationException(
                    "Select a booking.");

            var newQuantity = AskQuantity(ticket.Quantity);

            if (newQuantity is null)
                return;

            await Change(
                () => client.Adjust(
                    ticket.Id,
                    new AdjustTicketRequest(newQuantity.Value)),
                "Booking quantity updated.");
        });
    }

    private void UpdateAccount()
    {
        var user = client.Session?.User;

        accountLabel.Text = user is null
            ? "Browsing as guest"
            : $"{user.Name} • {user.Role}";

        accountButton.Visible = user is null;
        logoutButton.Visible = user is not null;

        updatingView = true;

        try
        {
            eventView.Items.Clear();
            eventView.Items.Add("Upcoming events");

            if (IsAdmin || IsManager)
                eventView.Items.Add("My event requests");

            if (IsAdmin)
                eventView.Items.Add("All event requests");

            eventView.SelectedIndex = 0;
        }
        finally
        {
            updatingView = false;
        }

        createButton.Visible = IsAdmin || IsManager;

        approveButton.Visible = IsAdmin;
        proposeButton.Visible = IsAdmin;
        rejectButton.Visible = IsAdmin;

        acceptButton.Visible = IsAdmin || IsManager;
        declineButton.Visible = IsAdmin || IsManager;

        allTicketsButton.Visible = IsAdmin;
        eventTicketsButton.Visible = IsAdmin || IsManager;
        adjustButton.Visible = IsAdmin;

        UpdateButtons();
    }

    private async Task RefreshEvents()
    {
        var selectedId = SelectedEvent?.Id;
        var view = eventView.SelectedItem?.ToString();

        EventDto[] items;

        if (view == "All event requests")
            items = await client.ReviewEvents();
        else if (view == "My event requests")
            items = await client.MyEvents();
        else
            items = await client.Events();

        eventsGrid.DataSource = items;

        if (selectedId is not null)
        {
            foreach (DataGridViewRow row in eventsGrid.Rows)
            {
                if (row.DataBoundItem is EventDto item
                    && item.Id == selectedId)
                {
                    eventsGrid.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        ShowSessions();

        statusLabel.Text = items.Length == 0
            ? "No events in this view."
            : $"{items.Length} event(s) loaded.";
    }

    private void ShowSessions()
    {
        var item = SelectedEvent;
        var canSeeProposal = item is not null
            && (IsAdmin || item.ManagerId == client.Session?.User.Id);

        showProposal.Visible = canSeeProposal
            && item?.ProposedSessions is not null;

        var viewingProposal = showProposal.Visible
            && showProposal.Checked;

        var sessions = viewingProposal
            ? item?.ProposedSessions
            : item?.Sessions;

        sessionsGrid.DataSource = sessions?
            .OrderBy(session => session.StartsAt)
            .Select(session => session with
            {
                StartsAt = session.StartsAt.ToLocalTime(),
                EndsAt = session.EndsAt.ToLocalTime()
            })
            .ToList();

        var availableColumn = sessionsGrid.Columns
            .Cast<DataGridViewColumn>()
            .Single(column =>
                column.DataPropertyName ==
                nameof(EventSessionDto.AvailableTickets));

        availableColumn.Visible =
            item?.Status == EventStatuses.Approved
            && !viewingProposal;

        scheduleNote.Text = item is null
            ? "Select an event to view its sessions."
            : viewingProposal
                ? $"Proposed schedule. {item.AdminNote}"
                : $"Original schedule • {item.Status}"
                    + (canSeeProposal
                        && !string.IsNullOrWhiteSpace(item.AdminNote)
                            ? $" • {item.AdminNote}"
                            : "");

        UpdateButtons();
    }

    private async Task RefreshBookings()
    {
        if (client.Session is null)
        {
            ticketsGrid.DataSource = null;
            UpdateButtons();
            return;
        }

        TicketDto[] items;

        if (bookingView == "all")
        {
            items = await client.AllTickets();
        }
        else if (bookingView == "event" && bookingEventId is Guid eventId)
        {
            items = await client.EventTickets(eventId);
        }
        else
        {
            items = await client.Tickets();
        }

        ticketsGrid.DataSource = items
            .Select(ticket => ticket with
            {
                StartsAt = ticket.StartsAt.ToLocalTime()
            })
            .ToList();

        statusLabel.Text = items.Length == 0
            ? "No bookings in this view."
            : $"{items.Length} booking(s) loaded.";

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var user = client.Session?.User;
        var item = SelectedEvent;
        var session = SelectedSession;
        var ticket = SelectedTicket;

        var pending = item?.Status == EventStatuses.Pending;
        var awaiting = item?.Status == EventStatuses.AwaitingManager;
        var ownsEvent = user is not null && item?.ManagerId == user.Id;

        approveButton.Enabled = IsAdmin && pending;
        proposeButton.Enabled = IsAdmin && (pending || awaiting);
        rejectButton.Enabled = IsAdmin && (pending || awaiting);
        acceptButton.Visible = ownsEvent && awaiting;
        declineButton.Visible = ownsEvent && awaiting;
        acceptButton.Enabled = ownsEvent && awaiting;
        declineButton.Enabled = ownsEvent && awaiting;

        bookButton.Enabled = user is not null
            && item?.Status == EventStatuses.Approved
            && session is not null
            && session.StartsAt > DateTimeOffset.UtcNow
            && session.AvailableTickets >= quantity.Value
            && !(showProposal.Visible && showProposal.Checked);

        myTicketsButton.Enabled = user is not null;
        allTicketsButton.Enabled = IsAdmin;

        eventTicketsButton.Enabled =
            item is not null && (IsAdmin || ownsEvent);

        var activeBooking = ticket is not null
            && ticket.Status == TicketStatuses.Booked
            && ticket.StartsAt > DateTimeOffset.UtcNow;

        cancelButton.Enabled = activeBooking
            && user is not null
            && (IsAdmin || ticket!.UserId == user.Id);

        adjustButton.Enabled = activeBooking && IsAdmin;
    }

    private async Task Change(Func<Task> action, string message)
    {
        await action();
        await RefreshEvents();
        await RefreshBookings();

        statusLabel.Text = message;
    }

    private async Task Run(Func<Task> action)
    {
        if (busy)
            return;

        busy = true;
        Enabled = false;

        try
        {
            statusLabel.ForeColor = DarkTheme.Muted;
            await action();
        }
        catch (Exception ex)
        {
            statusLabel.Text = ex.Message;
            statusLabel.ForeColor = Color.FromArgb(255, 145, 145);
        }
        finally
        {
            busy = false;
            Enabled = true;
            UpdateButtons();
        }
    }

    private EventDto RequireEvent()
    {
        return SelectedEvent
            ?? throw new InvalidOperationException(
                "Select an event on the Events page first.");
    }

    private bool Confirm(string text)
    {
        return MessageBox.Show(
            FindForm(),
            text,
            "VibeHive",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes;
    }

    private int? AskQuantity(int current)
    {
        using var dialog = new Form
        {
            Text = "Adjust booking",
            ClientSize = new Size(330, 180),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var input = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 10,
            Value = current,
            Width = 100
        };

        var save = DarkTheme.Button("Save");
        save.DialogResult = DialogResult.OK;

        var cancel = DarkTheme.Button("Cancel");
        cancel.DialogResult = DialogResult.Cancel;

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        layout.Controls.Add(
            DarkTheme.Label("New total ticket quantity"));

        layout.Controls.Add(input);
        layout.Controls.Add(Bar(save, cancel));
        dialog.Controls.Add(layout);

        DarkTheme.Apply(dialog);

        dialog.AcceptButton = save;
        dialog.CancelButton = cancel;

        return dialog.ShowDialog(FindForm()) == DialogResult.OK
            ? (int)input.Value
            : null;
    }
}

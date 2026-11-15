// Yevgeniy Mazur
// Event schedule editor.

using System.ComponentModel;
using VibeHive.Contracts;

namespace VibeHive.WinForms.UI;

public sealed class ScheduleDialog : Form
{
    private readonly TextBox nameInput = new();
    private readonly TextBox genreInput = new();
    private readonly TextBox noteInput = new();

    private readonly ComboBox venueInput = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly DateTimePicker firstDate = new()
    {
        Format = DateTimePickerFormat.Short,
        Value = DateTime.Today.AddDays(1)
    };

    private readonly DateTimePicker lastDate = new()
    {
        Format = DateTimePickerFormat.Short,
        Value = DateTime.Today.AddDays(1)
    };

    private readonly DateTimePicker startTime = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "h:mm tt",
        ShowUpDown = true,
        Value = DateTime.Today.AddHours(20)
    };

    private readonly DateTimePicker endTime = new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "h:mm tt",
        ShowUpDown = true,
        Value = DateTime.Today.AddHours(23)
    };

    private readonly NumericUpDown capacityInput = new()
    {
        Minimum = 1,
        Maximum = 22000,
        ThousandsSeparator = true
    };

    private readonly DataGridView grid = DarkTheme.Grid();
    private readonly BindingList<SessionRow> rows = new();

    private readonly Label message = new()
    {
        AutoSize = true,
        MaximumSize = new Size(780, 0)
    };

    private readonly bool isProposal;

    public CreateEventRequest? NewEvent { get; private set; }
    public ReviseEventRequest? Proposal { get; private set; }

    public ScheduleDialog(EventDto? item = null)
    {
        isProposal = item is not null;

        Text = isProposal
            ? "VibeHive — Propose Changes"
            : "VibeHive — Request an Event";

        ClientSize = new Size(850, 720);
        MinimumSize = new Size(850, 720);
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(20)
        };

        for (var i = 0; i < 8; i++)
        {
            layout.RowStyles.Add(new RowStyle(
                i == 4 ? SizeType.Percent : SizeType.AutoSize,
                i == 4 ? 100 : 0));
        }

        var heading = DarkTheme.Label(
            isProposal ? "Revise the request" : "Plan your event",
            true);

        layout.Controls.Add(heading, 0, 0);

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2
        };

        for (var i = 0; i < 3; i++)
            details.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33.33f));

        AddField(details, 0, "Event name", nameInput);
        AddField(details, 1, "Genre", genreInput);
        AddField(details, 2, "Venue", venueInput);

        layout.Controls.Add(details, 0, 1);

        var schedule = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 5,
            RowCount = 2
        };

        for (var i = 0; i < 5; i++)
            schedule.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 20));

        AddField(schedule, 0, "First date", firstDate);
        AddField(schedule, 1, "Last date", lastDate);
        AddField(schedule, 2, "Start time", startTime);
        AddField(schedule, 3, "End time", endTime);
        AddField(schedule, 4, "Ticket capacity", capacityInput);

        layout.Controls.Add(schedule, 0, 2);

        var sessionActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true
        };

        var add = DarkTheme.Button("Add dates");
        var updateSelected = DarkTheme.Button("Update selected");
        var updateAll = DarkTheme.Button("Update all");
        var remove = DarkTheme.Button("Remove selected");
        var clear = DarkTheme.Button("Clear dates");

        updateSelected.Click += (_, _) => UpdateSessions(false);
        updateAll.Click += (_, _) => UpdateSessions(true);

        add.Click += (_, _) => AddDates();

        remove.Click += (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is SessionRow row)
                rows.Remove(row);
        };

        clear.Click += (_, _) => rows.Clear();

        sessionActions.Controls.AddRange(
    new Control[]
    {
        add,
        updateSelected,
        updateAll,
        remove,
        clear
    });

        layout.Controls.Add(sessionActions, 0, 3);

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Start",
            DataPropertyName = nameof(SessionRow.StartsAt),
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "ddd, MMM d yyyy h:mm tt"
            }
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "End",
            DataPropertyName = nameof(SessionRow.EndsAt),
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "ddd, MMM d yyyy h:mm tt"
            }
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Tickets",
            DataPropertyName = nameof(SessionRow.Capacity),
            FillWeight = 40
        });

        grid.DataSource = rows;
        layout.Controls.Add(grid, 0, 4);

        var notes = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Visible = isProposal
        };

        notes.Controls.Add(
            DarkTheme.Label("Explain the proposed changes"), 0, 0);

        noteInput.Dock = DockStyle.Top;
        noteInput.Multiline = true;
        noteInput.Height = 60;
        noteInput.MaxLength = 500;

        notes.Controls.Add(noteInput, 0, 1);
        layout.Controls.Add(notes, 0, 5);

        message.Margin = new Padding(4, 10, 4, 10);
        layout.Controls.Add(message, 0, 6);

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft
        };

        var save = DarkTheme.Button(
            isProposal ? "Send proposal" : "Submit request");

        var cancel = DarkTheme.Button("Cancel");
        cancel.DialogResult = DialogResult.Cancel;

        save.Click += (_, _) => Save();

        actions.Controls.Add(cancel);
        actions.Controls.Add(save);

        layout.Controls.Add(actions, 0, 7);
        Controls.Add(layout);

        DarkTheme.Apply(this);
        BackColor = DarkTheme.Background;
        layout.BackColor = DarkTheme.Background;
        heading.Font = new Font("Segoe UI", 17, FontStyle.Bold);

        nameInput.MaxLength = 120;
        genreInput.MaxLength = 60;

        venueInput.DisplayMember = nameof(VenueDto.Name);
        venueInput.DataSource = VenueCatalog.Venues.ToList();
        venueInput.SelectedIndexChanged += (_, _) => UpdateCapacity();

        if (item is not null)
        {
            nameInput.Text = item.Name;
            genreInput.Text = item.Genre;
            noteInput.Text = item.AdminNote ?? "";

            venueInput.SelectedItem = VenueCatalog.Venues
                .Single(v => v.Id == item.VenueId);

            nameInput.ReadOnly = true;
            genreInput.ReadOnly = true;
            venueInput.Enabled = false;

            foreach (var session in item.ProposedSessions ?? item.Sessions)
            {
                rows.Add(new SessionRow(
                    session.StartsAt.ToLocalTime(),
                    session.EndsAt.ToLocalTime(),
                    session.Capacity));
            }
        }

        UpdateCapacity();
        CancelButton = cancel;

        message.Text =
            "Add one session per date. For overnight events, end time may be earlier than start time.";
    }

    private static void AddField(
        TableLayoutPanel panel,
        int column,
        string label,
        Control input)
    {
        panel.Controls.Add(DarkTheme.Label(label), column, 0);

        input.Dock = DockStyle.Top;
        input.Margin = new Padding(4, 4, 8, 12);

        panel.Controls.Add(input, column, 1);
    }

    private void UpdateCapacity()
    {
        if (venueInput.SelectedItem is not VenueDto venue)
            return;

        capacityInput.Maximum = venue.MaxCapacity;
        capacityInput.Value = venue.MaxCapacity;
    }

    private void AddDates()
    {
        var first = firstDate.Value.Date;
        var last = lastDate.Value.Date;
        var days = (last - first).Days + 1;

        if (days < 1 || days > 31 || rows.Count + days > 31)
        {
            ShowError("Choose a date range with up to 31 total sessions.");
            return;
        }

        if (startTime.Value.TimeOfDay == endTime.Value.TimeOfDay)
        {
            ShowError("Start and end times must be different.");
            return;
        }

        var additions = new List<SessionRow>();

        for (var day = first; day <= last; day = day.AddDays(1))
        {
            var start = day + startTime.Value.TimeOfDay;
            var end = day + endTime.Value.TimeOfDay;

            if (end < start)
                end = end.AddDays(1);

            if (TimeZoneInfo.Local.IsInvalidTime(start)
                || TimeZoneInfo.Local.IsInvalidTime(end)
                || TimeZoneInfo.Local.IsAmbiguousTime(start)
                || TimeZoneInfo.Local.IsAmbiguousTime(end))
            {
                ShowError("Choose times outside the daylight-saving clock change.");
                return;
            }

            var row = new SessionRow(
                new DateTimeOffset(start),
                new DateTimeOffset(end),
                (int)capacityInput.Value);

            if (row.StartsAt <= DateTimeOffset.UtcNow)
            {
                ShowError("Sessions must start in the future.");
                return;
            }

            if (rows.Concat(additions).Any(existing =>
                row.StartsAt < existing.EndsAt
                && row.EndsAt > existing.StartsAt))
            {
                ShowError("These dates overlap a session already in the list.");
                return;
            }

            additions.Add(row);
        }

        foreach (var row in additions)
            rows.Add(row);

        message.ForeColor = DarkTheme.Muted;
        message.Text = "Dates added. Remove a session to replace its time or capacity.";
    }

    private void UpdateSessions(bool updateAll)
    {
        var selected = grid.CurrentRow?.DataBoundItem as SessionRow;

        if (rows.Count == 0 || (!updateAll && selected is null))
        {
            ShowError("Select a session to update.");
            return;
        }

        if (startTime.Value.TimeOfDay == endTime.Value.TimeOfDay)
        {
            ShowError("Start and end times must be different.");
            return;
        }

        var replacements = new List<SessionRow>();

        foreach (var row in rows)
        {
            if (!updateAll && !ReferenceEquals(row, selected))
            {
                replacements.Add(row);
                continue;
            }

            var date = row.StartsAt.LocalDateTime.Date;
            var start = date + startTime.Value.TimeOfDay;
            var end = date + endTime.Value.TimeOfDay;

            if (end < start)
                end = end.AddDays(1);

            if (TimeZoneInfo.Local.IsInvalidTime(start)
                || TimeZoneInfo.Local.IsInvalidTime(end)
                || TimeZoneInfo.Local.IsAmbiguousTime(start)
                || TimeZoneInfo.Local.IsAmbiguousTime(end))
            {
                ShowError("Choose times outside the daylight-saving clock change.");
                return;
            }

            var replacement = new SessionRow(
                new DateTimeOffset(start),
                new DateTimeOffset(end),
                (int)capacityInput.Value);

            if (replacement.StartsAt <= DateTimeOffset.UtcNow)
            {
                ShowError("Sessions must start in the future.");
                return;
            }

            replacements.Add(replacement);
        }

        var ordered = replacements
            .OrderBy(row => row.StartsAt)
            .ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].StartsAt < ordered[i - 1].EndsAt)
            {
                ShowError("The updated sessions would overlap.");
                return;
            }
        }

        rows.RaiseListChangedEvents = false;

        try
        {
            rows.Clear();

            foreach (var row in ordered)
                rows.Add(row);
        }
        finally
        {
            rows.RaiseListChangedEvents = true;
            rows.ResetBindings();
        }

        message.ForeColor = DarkTheme.Muted;
        message.Text = updateAll
            ? "All session times and capacities updated. Review the list before submitting."
            : "Selected session updated. Review the list before submitting.";
    }

    private void Save()
    {
        if (nameInput.Text.Trim().Length < 2
            || string.IsNullOrWhiteSpace(genreInput.Text))
        {
            ShowError("Enter an event name and genre.");
            return;
        }

        if (venueInput.SelectedItem is not VenueDto venue)
        {
            ShowError("Select a venue.");
            return;
        }

        if (rows.Count == 0)
        {
            ShowError("Add at least one session.");
            return;
        }

        if (rows.Any(row =>
            row.StartsAt <= DateTimeOffset.UtcNow
            || row.Capacity > venue.MaxCapacity))
        {
            ShowError("Check session dates and the selected venue's capacity.");
            return;
        }

        var sessions = rows
            .OrderBy(row => row.StartsAt)
            .Select(row => new EventSessionRequest(
                row.StartsAt,
                row.EndsAt,
                row.Capacity))
            .ToList();

        if (isProposal)
        {
            if (string.IsNullOrWhiteSpace(noteInput.Text))
            {
                ShowError("Explain the proposed changes.");
                return;
            }

            Proposal = new ReviseEventRequest(
                noteInput.Text.Trim(),
                sessions);
        }
        else
        {
            NewEvent = new CreateEventRequest(
                nameInput.Text.Trim(),
                genreInput.Text.Trim(),
                venue.Id,
                sessions);
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowError(string text)
    {
        message.Text = text;
        message.ForeColor = Color.FromArgb(255, 145, 145);
    }

    private sealed record SessionRow(
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt,
        int Capacity);
}

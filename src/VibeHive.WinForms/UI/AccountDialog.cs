// Yevgeniy Mazur
// Sign-in and registration window.

using VibeHive.Contracts;
using VibeHive.WinForms.Services;

namespace VibeHive.WinForms.UI;

public sealed class AccountDialog : Form
{
    private readonly EventsClient client;

    private readonly TextBox nameInput = new();
    private readonly TextBox emailInput = new();
    private readonly TextBox passwordInput = new()
    {
        UseSystemPasswordChar = true
    };

    private readonly ComboBox roleInput = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    private readonly CheckBox registerMode = new()
    {
        Text = "Create a new account",
        AutoSize = true
    };

    private readonly Label nameLabel = DarkTheme.Label("Name");
    private readonly Label roleLabel = DarkTheme.Label("Account role");

    private readonly Label message = new()
    {
        AutoSize = true,
        MaximumSize = new Size(380, 0),
        Margin = new Padding(4, 10, 4, 10)
    };

    private readonly Button submit = DarkTheme.Button("Sign in");
    private bool busy;

    public AccountDialog(EventsClient client)
    {
        this.client = client;

        Text = "VibeHive — Account";
        ClientSize = new Size(440, 540);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            Padding = new Padding(24),
            AutoScroll = true
        };

        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 100));

        var heading = DarkTheme.Label("Welcome to VibeHive", true);

        roleInput.Items.AddRange(new object[]
        {
            UserRoles.User,
            UserRoles.EventManager,
            UserRoles.Admin
        });

        roleInput.SelectedIndex = 0;

        AddRow(layout, heading);
        AddRow(layout, registerMode);
        AddRow(layout, nameLabel);
        AddRow(layout, nameInput);
        AddRow(layout, DarkTheme.Label("Email"));
        AddRow(layout, emailInput);
        AddRow(layout, DarkTheme.Label("Password"));
        AddRow(layout, passwordInput);
        AddRow(layout, roleLabel);
        AddRow(layout, roleInput);
        AddRow(layout, message);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        var cancel = DarkTheme.Button("Cancel");
        cancel.DialogResult = DialogResult.Cancel;

        actions.Controls.Add(cancel);
        actions.Controls.Add(submit);

        AddRow(layout, actions);
        Controls.Add(layout);

        DarkTheme.Apply(this);
        BackColor = DarkTheme.Background;
        layout.BackColor = DarkTheme.Background;
        heading.Font = new Font("Segoe UI", 17, FontStyle.Bold);

        AcceptButton = submit;
        CancelButton = cancel;

        registerMode.CheckedChanged += (_, _) => UpdateMode();
        submit.Click += async (_, _) => await Submit();

        UpdateMode();
    }

    private static void AddRow(
        TableLayoutPanel layout,
        Control control)
    {
        var row = layout.RowCount;
        layout.RowCount++;

        layout.RowStyles.Add(
            new RowStyle(SizeType.AutoSize));

        control.Dock = DockStyle.Top;
        control.Margin = new Padding(4, 4, 4, 6);

        layout.Controls.Add(control, 0, row);
    }

    private void UpdateMode()
    {
        var registering = registerMode.Checked;

        nameLabel.Visible = nameInput.Visible = registering;
        roleLabel.Visible = roleInput.Visible = registering;

        submit.Text = registering ? "Register & sign in" : "Sign in";

        message.Text = registering
            ? "Use at least 8 characters for your password."
            : "Enter your email and password.";

        message.ForeColor = DarkTheme.Muted;
    }

    private async Task Submit()
    {
        if (busy)
            return;

        var email = emailInput.Text.Trim();
        var password = passwordInput.Text;

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Enter your email and password.");
            return;
        }

        if (registerMode.Checked)
        {
            if (nameInput.Text.Trim().Length < 2)
            {
                ShowError("Enter a name with at least 2 characters.");
                return;
            }

            if (password.Length < 8 || password.Length > 128)
            {
                ShowError("Use a password between 8 and 128 characters.");
                return;
            }
        }

        busy = true;
        SetInputsEnabled(false);
        message.Text = "Connecting...";
        message.ForeColor = DarkTheme.Muted;

        try
        {
            if (registerMode.Checked)
            {
                await client.Register(new RegisterRequest(
                    nameInput.Text.Trim(),
                    email,
                    password,
                    roleInput.SelectedItem!.ToString()!));

                registerMode.Checked = false;
            }

            await client.Login(new LoginRequest(email, password));

            passwordInput.Clear();
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            busy = false;

            if (!IsDisposed)
                SetInputsEnabled(true);
        }
    }

    private void SetInputsEnabled(bool enabled)
    {
        nameInput.Enabled = enabled;
        emailInput.Enabled = enabled;
        passwordInput.Enabled = enabled;
        roleInput.Enabled = enabled;
        registerMode.Enabled = enabled;
        submit.Enabled = enabled;
    }

    private void ShowError(string text)
    {
        message.Text = text;
        message.ForeColor = Color.FromArgb(255, 145, 145);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (busy && DialogResult != DialogResult.OK)
            e.Cancel = true;

        base.OnFormClosing(e);
    }
}

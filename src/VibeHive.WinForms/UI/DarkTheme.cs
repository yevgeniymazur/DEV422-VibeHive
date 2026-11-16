// Yevgeniy Mazur
// Dark theme controls.

namespace VibeHive.WinForms.UI;

public static class DarkTheme
{
    public static readonly Color Background = Color.FromArgb(20, 23, 31);
    public static readonly Color Surface = Color.FromArgb(30, 35, 46);
    public static readonly Color Input = Color.FromArgb(40, 46, 59);
    public static readonly Color Text = Color.FromArgb(235, 239, 247);
    public static readonly Color Muted = Color.FromArgb(160, 172, 192);
    public static readonly Color Accent = Color.FromArgb(100, 90, 230);

    public static void Apply(Control control)
    {
        control.Font = new Font("Segoe UI", 10);
        control.BackColor = Surface;
        control.ForeColor = Text;

        foreach (Control child in control.Controls)
            Apply(child);

        switch (control)
        {
            case Button button:
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.BackColor = Accent;
                button.ForeColor = Color.White;
                button.Padding = new Padding(8, 4, 8, 4);
                button.Cursor = Cursors.Hand;
                break;

            case TextBox textBox:
                textBox.BackColor = Input;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;

            case ComboBox comboBox:
                comboBox.BackColor = Input;
                comboBox.ForeColor = Text;
                comboBox.FlatStyle = FlatStyle.Flat;
                break;

            case NumericUpDown number:
                number.BackColor = Input;
                number.ForeColor = Text;
                break;

            case DataGridView grid:
                StyleGrid(grid);
                break;
        }
    }

    public static Button Button(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(95, 36),
            Margin = new Padding(4)
        };

        Apply(button);

        return button;
    }

    public static Label Label(string text, bool heading = false)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            ForeColor = heading ? Text : Muted,
            BackColor = Color.Transparent,
            Font = new Font(
                "Segoe UI",
                heading ? 14 : 10,
                heading ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(4, 8, 4, 8)
        };
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BorderStyle = BorderStyle.None
        };

        StyleGrid(grid);

        return grid;
    }

    private static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.ForeColor = Text;
        grid.GridColor = Input;
        grid.EnableHeadersVisualStyles = false;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight = 38;
        grid.RowTemplate.Height = 34;

        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Input,
            ForeColor = Muted,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            SelectionBackColor = Input,
            SelectionForeColor = Text
        };

        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Surface,
            ForeColor = Text,
            SelectionBackColor = Color.FromArgb(65, 60, 120),
            SelectionForeColor = Color.White,
            Padding = new Padding(5)
        };

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            Color.FromArgb(34, 39, 51);
    }
}

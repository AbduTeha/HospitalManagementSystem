using System.Drawing.Drawing2D;

namespace HospitalManagementSystem.UI;

internal static class UIHelpers
{
    // Color Scheme
    private static readonly Color PrimaryColor = Color.FromArgb(0, 102, 153);      // Professional Blue
    private static readonly Color PrimaryLight = Color.FromArgb(230, 242, 251);    // Light Blue
    private static readonly Color SecondaryColor = Color.FromArgb(76, 175, 80);    // Green (Success)
    private static readonly Color DangerColor = Color.FromArgb(244, 67, 54);       // Red (Danger)
    private static readonly Color TextColor = Color.FromArgb(33, 33, 33);          // Dark Text
    private static readonly Color BorderColor = Color.FromArgb(200, 200, 200);     // Light Gray Border
    private static readonly Color BackgroundColor = Color.FromArgb(248, 248, 248); // Off-white Background

    public static Button NavButton(string text)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 48,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F),
            Cursor = Cursors.Hand,
            ForeColor = TextColor,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            BackColor = Color.White,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = PrimaryLight, MouseDownBackColor = PrimaryColor }
        };
        return button;
    }

    public static Label Label(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = TextColor,
        Margin = new Padding(4),
        BackColor = Color.Transparent
    };

    public static TextBox TextBox() => new()
    {
        Width = 220,
        Height = 32,
        Font = new Font("Segoe UI", 10F),
        ForeColor = TextColor,
        BackColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle
    };

    public static ComboBox ComboBox() => new()
    {
        Width = 220,
        Height = 32,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = new Font("Segoe UI", 10F),
        ForeColor = TextColor,
        BackColor = Color.White,
        FlatStyle = FlatStyle.Flat
    };

    public static Button ActionButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Padding = new Padding(16, 0, 16, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            ForeColor = Color.White,
            BackColor = PrimaryColor,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(0, 82, 133), MouseDownBackColor = Color.FromArgb(0, 62, 113) }
        };
        return button;
    }

    public static Button SuccessButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Padding = new Padding(16, 0, 16, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            ForeColor = Color.White,
            BackColor = SecondaryColor,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(56, 155, 60), MouseDownBackColor = Color.FromArgb(36, 135, 40) }
        };
        return button;
    }

    public static Button DangerButton(string text)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 36,
            Padding = new Padding(16, 0, 16, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            ForeColor = Color.White,
            BackColor = DangerColor,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(224, 47, 34), MouseDownBackColor = Color.FromArgb(204, 27, 14) }
        };
        return button;
    }

    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
        RowHeadersVisible = false,
        Font = new Font("Segoe UI", 9F),
        ForeColor = TextColor,
        GridColor = BorderColor,
        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = PrimaryLight }
    };

    public static Panel Header(string title)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(20, 6, 20, 4),
            BackColor = PrimaryLight,
            BorderStyle = BorderStyle.FixedSingle
        };
        panel.Controls.Add(new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
            ForeColor = PrimaryColor,
            BackColor = Color.Transparent
        });
        return panel;
    }

    public static Panel CreateFieldPanel(string label, Control control)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(6),
            BackColor = Color.White
        };

        var labelControl = new Label
        {
            Text = label,
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = TextColor,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 4)
        };

        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 0, 0, 6);

        panel.Controls.Add(control);
        panel.Controls.Add(labelControl);

        return panel;
    }
}

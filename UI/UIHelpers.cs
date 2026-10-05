using System.Drawing.Drawing2D;

namespace HospitalManagementSystem.UI;

internal static class UIHelpers
{
    public static Button NavButton(string text) => new()
    {
        Text = text, Dock = DockStyle.Top, Height = 48, FlatStyle = FlatStyle.Flat,
        FlatAppearance = { BorderSize = 0 }, TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(18, 0, 0, 0), Font = new Font("Segoe UI", 10F), Cursor = Cursors.Hand
    };

    public static Label Label(string text) => new() { Text = text, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(4) };
    public static TextBox TextBox() => new() { Width = 220, Height = 28, Font = new Font("Segoe UI", 10F) };
    public static ComboBox ComboBox() => new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
    public static Button ActionButton(string text) => new() { Text = text, AutoSize = true, Height = 34, Padding = new Padding(12, 0, 12, 0), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };

    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        RowHeadersVisible = false, Font = new Font("Segoe UI", 9F)
    };

    public static Panel Header(string title)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(20, 14, 20, 8) };
        panel.Controls.Add(new Label { Text = title, AutoSize = true, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold) });
        return panel;
    }
}

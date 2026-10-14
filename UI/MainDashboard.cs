using HospitalManagementSystem.Core.Security;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class MainDashboard : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(12) };
    private readonly Panel _sidebar = new() { Dock = DockStyle.Left, Width = 220, Padding = new Padding(0, 20, 0, 0) };
    private readonly Label _userLabel = new() { Dock = DockStyle.Top, Height = 72, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold) };
    private readonly Dictionary<string, Button> _buttons = new();

    public MainDashboard()
    {
        Text = "Hospital Management System";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        BackColor = Color.FromArgb(248, 248, 248);
        InitializeCustomComponents();
        ApplyRolePermissions();
        ShowHome();
    }

    private void InitializeCustomComponents()
    {
        var session = SessionManager.Current ?? throw new InvalidOperationException("No active session.");

        _content.BackColor = Color.FromArgb(248, 248, 248);
        _sidebar.BackColor = Color.FromArgb(240, 245, 250);
        _sidebar.BorderStyle = BorderStyle.FixedSingle;
        _sidebar.Width = 250;

        _userLabel.Text = $"{session.FullName}\n{session.Role}";
        _userLabel.Height = 80;
        _userLabel.BackColor = Color.FromArgb(0, 102, 153);
        _userLabel.ForeColor = Color.White;
        _userLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        _userLabel.BorderStyle = BorderStyle.FixedSingle;

        _sidebar.Controls.Add(_userLabel);
        Controls.Add(_content);
        Controls.Add(_sidebar);

        AddNav("📊 Dashboard", ShowHome);
        AddNav("👥 Patients", () => OpenChild(new PatientForm()));
        AddNav("🏥 Doctors", () => OpenChild(new DoctorForm()));
        AddNav("📅 Appointments", () => OpenChild(new AppointmentForm()));
        AddNav("🛏️ Rooms", () => OpenChild(new RoomForm()));
        AddNav("💳 Billing", () => OpenChild(new BillingForm()));

        var spacer = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 245, 250) };
        _sidebar.Controls.Add(spacer);

        AddNav("🚪 Sign out", Close);

        Resize += (_, _) => _content.PerformLayout();
    }

    private void AddNav(string key, Action action)
    {
        var button = UIHelpers.NavButton(key);
        button.Click += (_, _) => action();
        _buttons[key] = button;
        _sidebar.Controls.Add(button);
    }

    private void ApplyRolePermissions()
    {
        _buttons["🏥 Doctors"].Visible = Permissions.Has(Permission.ManageDoctors);
        _buttons["🛏️ Rooms"].Visible = Permissions.Has(Permission.ManageRooms);
        _buttons["💳 Billing"].Visible = Permissions.Has(Permission.ManageBilling);
    }

    private void ShowHome()
    {
        ClearContent();
        var session = SessionManager.Current!;
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(UIHelpers.Header("Dashboard"));
        var summary = new Label
        {
            Text = $"Welcome, {session.FullName}!\n\nRole: {session.Role}\n\nUse the navigation menu to manage hospital operations.",
            Dock = DockStyle.Top,
            Height = 160,
            Padding = new Padding(22),
            Font = new Font("Segoe UI", 12F)
        };
        panel.Controls.Add(summary);
        var dbStatus = new Label { Dock = DockStyle.Top, Height = 40, Padding = new Padding(22, 0, 0, 0), Text = "Checking database connection..." };
        panel.Controls.Add(dbStatus);
        try
        {
            _ = new DBHelper().ExecuteScalar("SELECT 1;");
            dbStatus.Text = "Database connection: Connected";
        }
        catch (Exception ex) { dbStatus.Text = $"Database connection: Not available ({ex.Message})"; }
        _content.Controls.Add(panel);
    }

    private void OpenChild(Form form)
    {
        ClearContent();
        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;
        _content.Controls.Add(form);
        form.Show();
    }

    private void ClearContent()
    {
        foreach (Control control in _content.Controls)
        {
            if (control is Form form) form.Close();
            control.Dispose();
        }
        _content.Controls.Clear();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        SessionManager.End();
        base.OnFormClosed(e);
    }
}
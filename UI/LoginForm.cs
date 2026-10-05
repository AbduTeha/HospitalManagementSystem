using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class LoginForm : Form
{
    private readonly TextBox _username = UIHelpers.TextBox();
    private readonly TextBox _password = UIHelpers.TextBox();
    private readonly Button _login = UIHelpers.ActionButton("Sign in");
    private readonly ErrorProvider _errors = new();
    private readonly AuthenticationService _auth = new(new DBHelper());

    public LoginForm()
    {
        Text = "Hospital Management System - Sign In";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(500, 420);
        Size = new Size(560, 470);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(55, 45, 55, 45) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label { Text = "Hospital Management System", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold) };
        root.Controls.Add(title, 0, 0);
        _username.PlaceholderText = "Username";
        _password.PlaceholderText = "Password";
        _password.UseSystemPasswordChar = true;
        root.Controls.Add(_username, 0, 1);
        root.Controls.Add(_password, 0, 2);
        _login.Dock = DockStyle.Fill;
        _login.Click += LoginClicked;
        _password.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoginClicked(_login, EventArgs.Empty); };
        root.Controls.Add(_login, 0, 3);
        root.Controls.Add(new Label { Text = "Default accounts are documented in DatabaseSetup.sql.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.TopCenter, ForeColor = Color.DimGray }, 0, 4);
        Controls.Add(root);
        AcceptButton = _login;
    }

    private void LoginClicked(object? sender, EventArgs e)
    {
        _errors.Clear();
        if (string.IsNullOrWhiteSpace(_username.Text)) { _errors.SetError(_username, "Username is required."); return; }
        if (string.IsNullOrEmpty(_password.Text)) { _errors.SetError(_password, "Password is required."); return; }
        try
        {
            UseWaitCursor = true;
            var session = _auth.Authenticate(_username.Text, _password.Text);
            if (session is null)
            {
                MessageBox.Show("Invalid username or password.", "Sign In Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            SessionManager.Start(session);
            Hide();
            using var dashboard = new MainDashboard();
            dashboard.ShowDialog(this);
            SessionManager.End();
            Show();
            _password.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sign In Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }
}

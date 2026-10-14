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
        Size = new Size(560, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.White;
        InitializeCustomComponents();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(0, 102, 153),
            Padding = new Padding(20)
        };

        var title = new Label
        {
            Text = "Hospital Management\nSystem",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
            ForeColor = Color.White
        };
        header.Controls.Add(title);
        root.Controls.Add(header);

        var form = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 300,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(45, 30, 45, 30),
            BackColor = Color.White
        };

        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var usernameLabel = UIHelpers.Label("Username");
        usernameLabel.Dock = DockStyle.Fill;
        usernameLabel.AutoSize = false;
        usernameLabel.Height = 22;
        form.Controls.Add(usernameLabel, 0, 0);

        _username.PlaceholderText = "Enter your username";
        _username.Dock = DockStyle.Fill;
        form.Controls.Add(_username, 0, 1);

        var passwordLabel = UIHelpers.Label("Password");
        passwordLabel.Dock = DockStyle.Fill;
        passwordLabel.AutoSize = false;
        passwordLabel.Height = 22;
        form.Controls.Add(passwordLabel, 0, 2);

        _password.PlaceholderText = "Enter your password";
        _password.UseSystemPasswordChar = true;
        _password.Dock = DockStyle.Fill;
        form.Controls.Add(_password, 0, 3);

        root.Controls.Add(form);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(45, 10, 45, 15),
            BackColor = Color.White
        };

        _login.Click += LoginClicked;
        _login.Width = 150;
        _password.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoginClicked(_login, EventArgs.Empty); };
        buttonPanel.Controls.Add(_login);
        root.Controls.Add(buttonPanel);

        var footer = new Label
        {
            Text = "Default accounts are in DatabaseSetup.sql\nAdmin: admin / admin123 | Doctor: doc_john / doc123 | Receptionist: rec_alice / rec123",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8F),
            Padding = new Padding(20),
            BackColor = Color.FromArgb(248, 248, 248)
        };
        root.Controls.Add(footer);

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

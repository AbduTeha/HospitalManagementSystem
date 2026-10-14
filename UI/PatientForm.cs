using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class PatientForm : Form
{
    private readonly PatientRepository _repo = new(new DBHelper());
    private readonly DataGridView _grid = UIHelpers.Grid();
    private readonly TextBox _name = UIHelpers.TextBox();
    private readonly ComboBox _gender = UIHelpers.ComboBox();
    private readonly DateTimePicker _dob = new() { Width = 220, Format = DateTimePickerFormat.Short, MaxDate = DateTime.Today };
    private readonly TextBox _phone = UIHelpers.TextBox();
    private readonly TextBox _address = UIHelpers.TextBox();
    private readonly ComboBox _blood = UIHelpers.ComboBox();
    private readonly TextBox _search = UIHelpers.TextBox();
    private readonly ErrorProvider _errors = new();
    private int _selectedId;

    public PatientForm()
    {
        Text = "Patients";
        InitializeCustomComponents();
        LoadPatients();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 248, 248) };

        var editor = new Panel
        {
            Dock = DockStyle.Top,
            Height = 280,
            BackColor = Color.White,
            Padding = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };

        var formTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(0)
        };

        for (var i = 0; i < 4; i++)
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var i = 0; i < 3; i++)
            formTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

        AddField(formTable, "Full Name *", _name, 0, 0);
        AddField(formTable, "Gender *", _gender, 1, 0);
        AddField(formTable, "Date of Birth *", _dob, 2, 0);
        AddField(formTable, "Phone *", _phone, 3, 0);

        AddField(formTable, "Address *", _address, 0, 1);
        AddField(formTable, "Blood Group *", _blood, 1, 1);

        _gender.Items.AddRange(new[] { "Male", "Female", "Other" });
        _gender.SelectedIndex = 0;
        _blood.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" });
        _blood.SelectedIndex = 0;

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(6),
            BackColor = Color.White,
            AutoSize = true
        };

        var add = UIHelpers.SuccessButton("➕ Add");
        add.Click += (_, _) => AddPatient();
        var update = UIHelpers.ActionButton("✏️ Update");
        update.Click += (_, _) => UpdatePatient();
        var delete = UIHelpers.DangerButton("🗑️ Delete");
        delete.Click += (_, _) => DeletePatient();
        var clear = UIHelpers.ActionButton("🔄 Clear");
        clear.Click += (_, _) => ClearEditor();

        buttonPanel.Controls.AddRange(new Control[] { add, update, delete, clear });
        formTable.Controls.Add(buttonPanel, 0, 2);
        formTable.SetColumnSpan(buttonPanel, 2);

        var searchPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(6),
            BackColor = Color.White,
            AutoSize = true
        };

        _search.PlaceholderText = "Search by name or phone...";
        _search.Width = 250;
        var searchBtn = UIHelpers.ActionButton("🔍 Search");
        searchBtn.Click += (_, _) => LoadPatients();
        var allBtn = UIHelpers.ActionButton("📋 All");
        allBtn.Click += (_, _) => { _search.Clear(); LoadPatients(); };

        searchPanel.Controls.AddRange(new Control[] { _search, searchBtn, allBtn });
        formTable.Controls.Add(searchPanel, 2, 2);
        formTable.SetColumnSpan(searchPanel, 2);

        editor.Controls.Add(formTable);

        var headerSection = new Panel { Dock = DockStyle.Fill, Height = 55, BackColor = Color.White, Padding = new Padding(0) };
        headerSection.Controls.Add(UIHelpers.Header("Patient Management"));

        // Use a layout with three rows: editor (auto), header (auto), grid (fill)
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(editor, 0, 0);
        // add small margin to header to sit a bit lower
        headerSection.Margin = new Padding(0, 8, 0, 0);
        layout.Controls.Add(headerSection, 0, 1);
        layout.Controls.Add(_grid, 0, 2);

        root.Controls.Add(layout);
        _grid.CellClick += GridCellClick;
        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel table, string label, Control control, int column, int row)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6), BackColor = Color.White };

        var labelControl = new Label
        {
            Text = label,
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            BackColor = Color.White,
            Margin = new Padding(0, 0, 0, 4)
        };

        control.Dock = DockStyle.Top;
        control.Margin = new Padding(0, 0, 0, 6);

        panel.Controls.Add(control);
        panel.Controls.Add(labelControl);
        table.Controls.Add(panel, column, row);
    }

    private void LoadPatients()
    {
        try
        {
            var data = _repo.GetAll(_search.Text);
            _grid.DataSource = data.Select(p => new { p.ID, p.FullName, p.Gender, DOB = p.DOB.ToString("yyyy-MM-dd"), p.Phone, p.Address, p.BloodGroup, Registered = p.RegisteredAt.ToString("yyyy-MM-dd") }).ToList();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Patients", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private Patient ReadEditor() => new()
    {
        ID = _selectedId, FullName = _name.Text, Gender = _gender.Text, DOB = _dob.Value.Date,
        Phone = _phone.Text, Address = _address.Text, BloodGroup = _blood.Text
    };

    private void AddPatient()
    {
        try { _errors.Clear(); var p = ReadEditor(); _repo.Add(p); ClearEditor(); LoadPatients(); MessageBox.Show("Patient added successfully.", "Patients", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Patients", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void UpdatePatient()
    {
        if (_selectedId <= 0) { MessageBox.Show("Select a patient first."); return; }
        try { var p = ReadEditor(); if (!_repo.Update(p)) throw new InvalidOperationException("Patient was not updated."); ClearEditor(); LoadPatients(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Patients", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void DeletePatient()
    {
        if (_selectedId <= 0) { MessageBox.Show("Select a patient first."); return; }
        if (MessageBox.Show("Delete the selected patient? Related appointments will also be deleted.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try { _repo.Delete(_selectedId); ClearEditor(); LoadPatients(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Patients", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void GridCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var p = _repo.GetAll(_search.Text).FirstOrDefault(x => x.ID == Convert.ToInt32(_grid.Rows[e.RowIndex].Cells["ID"].Value));
        if (p is null) return;
        _selectedId = p.ID; _name.Text = p.FullName; _gender.SelectedItem = p.Gender; _dob.Value = p.DOB; _phone.Text = p.Phone; _address.Text = p.Address; _blood.SelectedItem = p.BloodGroup;
    }

    private void ClearEditor()
    {
        _selectedId = 0; _name.Clear(); _phone.Clear(); _address.Clear(); _gender.SelectedIndex = 0; _blood.SelectedIndex = 0; _dob.Value = DateTime.Today;
    }
}

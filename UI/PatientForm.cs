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
        var root = new Panel { Dock = DockStyle.Fill };
        root.Controls.Add(_grid);
        var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 205, ColumnCount = 4, RowCount = 4, Padding = new Padding(8) };
        for (var i = 0; i < 4; i++) editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        AddField(editor, "Full name", _name, 0, 0); AddField(editor, "Gender", _gender, 1, 0); AddField(editor, "DOB", _dob, 2, 0); AddField(editor, "Phone", _phone, 3, 0);
        AddField(editor, "Address", _address, 0, 1); AddField(editor, "Blood group", _blood, 1, 1);
        _gender.Items.AddRange(new[] { "Male", "Female", "Other" }); _gender.SelectedIndex = 0;
        _blood.Items.AddRange(new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" }); _blood.SelectedIndex = 0;
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        var add = UIHelpers.ActionButton("Add"); add.Click += (_, _) => AddPatient();
        var update = UIHelpers.ActionButton("Update"); update.Click += (_, _) => UpdatePatient();
        var delete = UIHelpers.ActionButton("Delete"); delete.Click += (_, _) => DeletePatient();
        var clear = UIHelpers.ActionButton("Clear"); clear.Click += (_, _) => ClearEditor();
        actions.Controls.AddRange(new Control[] { add, update, delete, clear });
        editor.Controls.Add(actions, 0, 2); editor.SetColumnSpan(actions, 2);
        var searchPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
        _search.PlaceholderText = "Search name or phone"; var search = UIHelpers.ActionButton("Search"); search.Click += (_, _) => LoadPatients();
        var all = UIHelpers.ActionButton("All"); all.Click += (_, _) => { _search.Clear(); LoadPatients(); };
        searchPanel.Controls.AddRange(new Control[] { _search, search, all }); editor.Controls.Add(searchPanel, 2, 2); editor.SetColumnSpan(searchPanel, 2);
        root.Controls.Add(editor); root.Controls.Add(UIHelpers.Header("Patient Management"));
        _grid.BringToFront();
        _grid.CellClick += GridCellClick;
        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel table, string label, Control control, int column, int row)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
        panel.Controls.Add(control); panel.Controls.Add(new Label { Text = label, AutoSize = true, Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
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

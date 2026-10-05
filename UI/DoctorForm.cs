using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class DoctorForm : Form
{
    private readonly DoctorRepository _repo = new(new DBHelper());
    private readonly DataGridView _grid = UIHelpers.Grid();
    private readonly TextBox _name = UIHelpers.TextBox();
    private readonly TextBox _specialization = UIHelpers.TextBox();
    private readonly TextBox _phone = UIHelpers.TextBox();
    private readonly TextBox _email = UIHelpers.TextBox();
    private readonly NumericUpDown _fee = new() { Width = 220, DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Increment = 50 };
    private int _selectedId;

    public DoctorForm()
    {
        Text = "Doctors";
        InitializeCustomComponents();
        LoadDoctors();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill };
        root.Controls.Add(_grid);
        var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 185, ColumnCount = 4, RowCount = 3, Padding = new Padding(8) };
        for (var i = 0; i < 4; i++) editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        AddField(editor, "Full name", _name, 0, 0); AddField(editor, "Specialization", _specialization, 1, 0); AddField(editor, "Phone", _phone, 2, 0); AddField(editor, "Email", _email, 3, 0); AddField(editor, "Consultation fee", _fee, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var add = UIHelpers.ActionButton("Add"); add.Click += (_, _) => AddDoctor();
        var update = UIHelpers.ActionButton("Update"); update.Click += (_, _) => UpdateDoctor();
        var delete = UIHelpers.ActionButton("Delete"); delete.Click += (_, _) => DeleteDoctor();
        var clear = UIHelpers.ActionButton("Clear"); clear.Click += (_, _) => ClearEditor();
        actions.Controls.AddRange(new Control[] { add, update, delete, clear }); editor.Controls.Add(actions, 1, 1); editor.SetColumnSpan(actions, 3);
        root.Controls.Add(editor); root.Controls.Add(UIHelpers.Header("Doctor & Fee Management")); Controls.Add(root);
        _grid.CellClick += GridCellClick;
    }

    private static void AddField(TableLayoutPanel table, string label, Control control, int column, int row)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) }; panel.Controls.Add(control); panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }); table.Controls.Add(panel, column, row);
    }

    private void LoadDoctors()
    {
        try { _grid.DataSource = _repo.GetAll().Select(d => new { d.ID, d.FullName, d.Specialization, d.Phone, d.Email, Fee = d.ConsultationFee.ToString("0.00") }).ToList(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Doctors", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private Doctor ReadEditor() => new() { ID = _selectedId, FullName = _name.Text, Specialization = _specialization.Text, Phone = _phone.Text, Email = _email.Text, ConsultationFee = _fee.Value };

    private void AddDoctor() { try { _repo.Add(ReadEditor()); ClearEditor(); LoadDoctors(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Doctors", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void UpdateDoctor() { if (_selectedId <= 0) { MessageBox.Show("Select a doctor first."); return; } try { _repo.Update(ReadEditor()); ClearEditor(); LoadDoctors(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Doctors", MessageBoxButtons.OK, MessageBoxIcon.Error); } }
    private void DeleteDoctor() { if (_selectedId <= 0) { MessageBox.Show("Select a doctor first."); return; } if (MessageBox.Show("Delete selected doctor?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return; try { _repo.Delete(_selectedId); ClearEditor(); LoadDoctors(); } catch (Exception ex) { MessageBox.Show(ex.Message, "Doctors", MessageBoxButtons.OK, MessageBoxIcon.Error); } }

    private void GridCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return; var id = Convert.ToInt32(_grid.Rows[e.RowIndex].Cells["ID"].Value); var d = _repo.GetById(id); if (d is null) return;
        _selectedId = d.ID; _name.Text = d.FullName; _specialization.Text = d.Specialization; _phone.Text = d.Phone; _email.Text = d.Email ?? string.Empty; _fee.Value = d.ConsultationFee;
    }

    private void ClearEditor() { _selectedId = 0; _name.Clear(); _specialization.Clear(); _phone.Clear(); _email.Clear(); _fee.Value = 0; }
}

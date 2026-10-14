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
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 248, 248) };

        var editor = new Panel
        {
            Dock = DockStyle.Top,
            Height = 220,
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

        AddField(formTable, "Full Name *", _name, 0, 0);
        AddField(formTable, "Specialization *", _specialization, 1, 0);
        AddField(formTable, "Phone *", _phone, 2, 0);
        AddField(formTable, "Email", _email, 3, 0);
        AddField(formTable, "Consultation Fee *", _fee, 0, 1);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(6),
            BackColor = Color.White,
            AutoSize = true
        };

        var add = UIHelpers.SuccessButton("➕ Add");
        add.Click += (_, _) => AddDoctor();
        var update = UIHelpers.ActionButton("✏️ Update");
        update.Click += (_, _) => UpdateDoctor();
        var delete = UIHelpers.DangerButton("🗑️ Delete");
        delete.Click += (_, _) => DeleteDoctor();
        var clear = UIHelpers.ActionButton("🔄 Clear");
        clear.Click += (_, _) => ClearEditor();

        buttonPanel.Controls.AddRange(new Control[] { add, update, delete, clear });
        formTable.Controls.Add(buttonPanel, 1, 1);
        formTable.SetColumnSpan(buttonPanel, 3);

        editor.Controls.Add(formTable);

        var headerSection = new Panel { Dock = DockStyle.Fill, Height = 55, BackColor = Color.White, Padding = new Padding(0) };
        headerSection.Controls.Add(UIHelpers.Header("Doctor & Fee Management"));

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(editor, 0, 0);
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

using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class RoomForm : Form
{
    private readonly RoomService _service = new(new DBHelper());
    private readonly DataGridView _grid = UIHelpers.Grid();
    private readonly TextBox _number = UIHelpers.TextBox();
    private readonly ComboBox _type = UIHelpers.ComboBox();
    private readonly NumericUpDown _rate = new() { Width = 180, DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Increment = 50 };
    private readonly ComboBox _filter = UIHelpers.ComboBox();

    public RoomForm()
    {
        Text = "Rooms & Beds"; InitializeCustomComponents(); LoadRooms();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill }; root.Controls.Add(_grid);
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 125, Padding = new Padding(10) };
        _type.Items.AddRange(new[] { "General", "ICU", "Private" }); _type.SelectedIndex = 0;
        _filter.Items.AddRange(new[] { "All", "Available", "Occupied" }); _filter.SelectedIndex = 0; _filter.SelectedIndexChanged += (_, _) => LoadRooms();
        bar.Controls.AddRange(new Control[] { UIHelpers.Label("Room number"), _number, UIHelpers.Label("Type"), _type, UIHelpers.Label("Daily rate"), _rate });
        var add = UIHelpers.ActionButton("Add Room"); add.Click += (_, _) => AddRoom();
        var occupy = UIHelpers.ActionButton("Mark Occupied"); occupy.Click += (_, _) => SetStatus("Occupied");
        var release = UIHelpers.ActionButton("Mark Available"); release.Click += (_, _) => SetStatus("Available");
        bar.Controls.AddRange(new Control[] { add, occupy, release, UIHelpers.Label("Filter"), _filter });
        root.Controls.Add(bar); root.Controls.Add(UIHelpers.Header("Ward & Bed Allocation")); Controls.Add(root);
    }

    private void LoadRooms()
    {
        try { var status = _filter.SelectedItem?.ToString() == "All" ? null : _filter.SelectedItem?.ToString(); _grid.DataSource = _service.GetAll(status).Select(r => new { r.RoomID, r.RoomNumber, r.RoomType, DailyRate = r.DailyRate.ToString("0.00"), r.Status }).ToList(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Rooms", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void AddRoom()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_number.Text)) throw new ArgumentException("Room number is required.");
            _service.Add(new Room { RoomNumber = _number.Text.Trim(), RoomType = _type.Text, DailyRate = _rate.Value, Status = "Available" }); _number.Clear(); _rate.Value = 0; LoadRooms();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Rooms", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void SetStatus(string status)
    {
        if (_grid.CurrentRow is null) { MessageBox.Show("Select a room first."); return; }
        try { _service.SetStatus(Convert.ToInt32(_grid.CurrentRow.Cells["RoomID"].Value), status); LoadRooms(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Rooms", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

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
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 248, 248) };

        var toolbar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 180,
            BackColor = Color.White,
            Padding = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };

        var inputPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(0, 0, 0, 8),
            BackColor = Color.White
        };

        _type.Items.AddRange(new[] { "General", "ICU", "Private" });
        _type.SelectedIndex = 0;

        inputPanel.Controls.Add(UIHelpers.Label("Room #"));
        _number.Width = 100;
        inputPanel.Controls.Add(_number);
        inputPanel.Controls.Add(UIHelpers.Label("Type"));
        _type.Width = 120;
        inputPanel.Controls.Add(_type);
        inputPanel.Controls.Add(UIHelpers.Label("Daily Rate"));
        _rate.Width = 120;
        inputPanel.Controls.Add(_rate);


        toolbar.Controls.Add(inputPanel);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(0),
            BackColor = Color.White
        };

        var add = UIHelpers.SuccessButton("➕ Add Room");
        add.Click += (_, _) => AddRoom();
        var occupy = UIHelpers.ActionButton("🔴 Mark Occupied");
        occupy.Click += (_, _) => SetStatus("Occupied");
        var release = UIHelpers.SuccessButton("🟢 Mark Available");
        release.Click += (_, _) => SetStatus("Available");

        actionPanel.Controls.AddRange(new Control[] { add, occupy, release });
        toolbar.Controls.Add(actionPanel);

        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = Color.White
        };

        _filter.Items.AddRange(new[] { "All", "Available", "Occupied" });
        _filter.SelectedIndex = 0;
        _filter.SelectedIndexChanged += (_, _) => LoadRooms();
        _filter.Width = 150;

        filterPanel.Controls.Add(UIHelpers.Label("Filter"));
        filterPanel.Controls.Add(_filter);
        toolbar.Controls.Add(filterPanel);

        // Use a TableLayoutPanel to ensure toolbar/editor top, header beneath, and grid fills remainder.
        var headerSection = new Panel { Dock = DockStyle.Fill, Height = 55, BackColor = Color.White, Padding = new Padding(0) };
        headerSection.Controls.Add(UIHelpers.Header("Ward & Bed Allocation"));

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(toolbar, 0, 0);
        headerSection.Margin = new Padding(0, 8, 0, 0);
        layout.Controls.Add(headerSection, 0, 1);
        layout.Controls.Add(_grid, 0, 2);

        root.Controls.Add(layout);
        Controls.Add(root);
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

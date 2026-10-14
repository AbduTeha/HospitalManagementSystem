using System.Drawing.Printing;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class BillingForm : Form
{
    private readonly DBHelper _db = new();
    private readonly PatientRepository _patients;
    private readonly DoctorRepository _doctors;
    private readonly RoomService _rooms;
    private readonly BillingService _service;
    private readonly ComboBox _patient = UIHelpers.ComboBox();
    private readonly ComboBox _doctor = UIHelpers.ComboBox();
    private readonly ComboBox _room = UIHelpers.ComboBox();
    private readonly NumericUpDown _days = new() { Width = 150, Minimum = 1, Maximum = 365, Value = 1 };
    private readonly NumericUpDown _medicine = new() { Width = 150, DecimalPlaces = 2, Maximum = 1000000, Minimum = 0, Increment = 10 };
    private readonly Label _doctorFee = new() { AutoSize = true, Text = "0.00", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly Label _roomFee = new() { AutoSize = true, Text = "0.00", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly Label _total = new() { AutoSize = true, Text = "0.00", Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold) };
    private readonly ComboBox _payment = UIHelpers.ComboBox();
    private readonly DataGridView _grid = UIHelpers.Grid();
    private Bill? _lastBill;

    public BillingForm()
    {
        _patients = new PatientRepository(_db); _doctors = new DoctorRepository(_db); _rooms = new RoomService(_db); _service = new BillingService(_db);
        _service.OnBillGenerated += EventLogger.LogBill;
        Text = "Billing"; InitializeCustomComponents(); LoadSelectors(); LoadBills(); CalculateTotal();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 248, 248) };

        var editor = new Panel
        {
            Dock = DockStyle.Top,
            Height = 330,
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
            formTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            formTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));

        AddField(formTable, "Patient *", _patient, 0, 0);
        AddField(formTable, "Doctor *", _doctor, 1, 0);
        AddField(formTable, "Room *", _room, 2, 0);
        AddField(formTable, "Stay Days *", _days, 3, 0);

        AddField(formTable, "Doctor Fee", _doctorFee, 0, 1);
        AddField(formTable, "Room Fee", _roomFee, 1, 1);
        AddField(formTable, "Medicine Fee", _medicine, 2, 1);
        AddField(formTable, "Payment", _payment, 3, 1);

        _payment.Items.AddRange(new[] { "Unpaid", "Paid" });
        _payment.SelectedIndex = 0;

        var totalPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6), BackColor = Color.FromArgb(230, 242, 251) };
        var totalLabel = new Label
        {
            Text = "Total Amount",
            Dock = DockStyle.Top,
            Height = 22,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33),
            BackColor = Color.Transparent
        };
        _total.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
        _total.ForeColor = Color.FromArgb(0, 102, 153);
        _total.Dock = DockStyle.Top;
        _total.AutoSize = false;
        _total.Height = 40;
        totalPanel.Controls.Add(_total);
        totalPanel.Controls.Add(totalLabel);
        formTable.Controls.Add(totalPanel, 0, 2);

        var actionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(6),
            BackColor = Color.White,
            AutoSize = true
        };

        var generate = UIHelpers.SuccessButton("💳 Generate Bill");
        generate.Click += (_, _) => GenerateBill();
        var print = UIHelpers.ActionButton("🖨️ Print Receipt");
        print.Click += (_, _) => PrintReceipt();
        var refresh = UIHelpers.ActionButton("🔄 Refresh");
        refresh.Click += (_, _) => LoadBills();

        actionPanel.Controls.AddRange(new Control[] { generate, print, refresh });
        formTable.Controls.Add(actionPanel, 1, 2);
        formTable.SetColumnSpan(actionPanel, 3);

        _doctor.SelectedIndexChanged += (_, _) => CalculateTotal();
        _room.SelectedIndexChanged += (_, _) => CalculateTotal();
        _days.ValueChanged += (_, _) => CalculateTotal();
        _medicine.ValueChanged += (_, _) => CalculateTotal();

        editor.Controls.Add(formTable);

        var headerSection = new Panel { Dock = DockStyle.Fill, Height = 55, BackColor = Color.White, Padding = new Padding(0) };
        headerSection.Controls.Add(UIHelpers.Header("Billing & Invoice Generator"));

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(editor, 0, 0);
        headerSection.Margin = new Padding(0, 8, 0, 0);
        layout.Controls.Add(headerSection, 0, 1);
        layout.Controls.Add(_grid, 0, 2);

        root.Controls.Add(layout);
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

    private void LoadSelectors()
    {
        try
        {
            var patients = _patients.GetAll().ToList();
            if (patients.Count > 0)
            {
                _patient.DataSource = new BindingSource(patients, null);
                _patient.DisplayMember = "FullName";
                _patient.ValueMember = "ID";
            }

            var doctors = _doctors.GetAll().ToList();
            if (doctors.Count > 0)
            {
                _doctor.DataSource = new BindingSource(doctors, null);
                _doctor.DisplayMember = "FullName";
                _doctor.ValueMember = "ID";
            }

            var rooms = _rooms.GetAll("Available").ToList();
            if (rooms.Count > 0)
            {
                _room.DataSource = new BindingSource(rooms, null);
                _room.DisplayMember = "RoomNumber";
                _room.ValueMember = "RoomID";
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Billing", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void CalculateTotal()
    {
        var doctorFee = (_doctor.SelectedItem as Doctor)?.ConsultationFee ?? 0m;
        var roomRate = (_room.SelectedItem as Room)?.DailyRate ?? 0m;
        var roomFee = roomRate * _days.Value;
        _doctorFee.Text = doctorFee.ToString("0.00"); _roomFee.Text = roomFee.ToString("0.00"); _total.Text = (doctorFee + roomFee + _medicine.Value).ToString("0.00");
    }

    private void GenerateBill()
    {
        try
        {
            if (_patient.SelectedValue is null) throw new InvalidOperationException("Select a patient.");
            var doctorFee = (_doctor.SelectedItem as Doctor)?.ConsultationFee ?? 0m;
            var roomFee = (_room.SelectedItem as Room)?.DailyRate * _days.Value ?? 0m;
            var bill = new Bill { PatientID = Convert.ToInt32(_patient.SelectedValue), DoctorFee = doctorFee, RoomFee = roomFee, MedicineFee = _medicine.Value, PaymentStatus = _payment.Text };
            _service.GenerateBill(bill); _lastBill = bill; LoadBills(); MessageBox.Show($"Bill #{bill.BillID} generated. Total: {bill.TotalAmount:0.00}", "Billing", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Billing", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void LoadBills()
    {
        try { _grid.DataSource = _service.GetAll().Select(b => new { b.BillID, Patient = b.PatientName, DoctorFee = b.DoctorFee.ToString("0.00"), RoomFee = b.RoomFee.ToString("0.00"), MedicineFee = b.MedicineFee.ToString("0.00"), Total = b.TotalAmount.ToString("0.00"), b.PaymentStatus, Date = b.BillDate.ToString("yyyy-MM-dd HH:mm") }).ToList(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Billing", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void PrintReceipt()
    {
        if (_lastBill is null) { MessageBox.Show("Generate a bill before printing its receipt."); return; }
        var document = new PrintDocument(); document.PrintPage += (_, e) =>
        {
            using var font = new Font("Consolas", 10F);
            var lines = new[] { "HOSPITAL MANAGEMENT SYSTEM", "===========================", $"Bill #: {_lastBill.BillID}", $"Date: {_lastBill.BillDate:yyyy-MM-dd HH:mm}", $"Patient: {_patient.Text}", "---------------------------", $"Doctor fee : {_lastBill.DoctorFee,10:0.00}", $"Room fee   : {_lastBill.RoomFee,10:0.00}", $"Medicine   : {_lastBill.MedicineFee,10:0.00}", $"TOTAL      : {_lastBill.TotalAmount,10:0.00}", $"Payment    : {_lastBill.PaymentStatus}", "===========================", "Thank you." };
            var y = 40f; foreach (var line in lines) { e.Graphics!.DrawString(line, font, Brushes.Black, 40, y); y += 18; }
        };
        using var dialog = new PrintDialog { Document = document, UseEXDialog = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) document.Print();
        document.Dispose();
    }
}

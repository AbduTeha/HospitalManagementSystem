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
        var root = new Panel { Dock = DockStyle.Fill }; root.Controls.Add(_grid);
        var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 245, ColumnCount = 4, RowCount = 4, Padding = new Padding(8) };
        for (var i = 0; i < 4; i++) editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        AddField(editor, "Patient", _patient, 0, 0); AddField(editor, "Doctor", _doctor, 1, 0); AddField(editor, "Room", _room, 2, 0); AddField(editor, "Stay days", _days, 3, 0);
        AddField(editor, "Doctor fee", _doctorFee, 0, 1); AddField(editor, "Room fee", _roomFee, 1, 1); AddField(editor, "Medicine fee", _medicine, 2, 1); AddField(editor, "Payment", _payment, 3, 1);
        _payment.Items.AddRange(new[] { "Unpaid", "Paid" }); _payment.SelectedIndex = 0;
        var totalPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) }; totalPanel.Controls.Add(_total); totalPanel.Controls.Add(new Label { Text = "Total", Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }); editor.Controls.Add(totalPanel, 0, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var generate = UIHelpers.ActionButton("Generate Bill"); generate.Click += (_, _) => GenerateBill();
        var print = UIHelpers.ActionButton("Print Receipt"); print.Click += (_, _) => PrintReceipt();
        var refresh = UIHelpers.ActionButton("Refresh"); refresh.Click += (_, _) => LoadBills();
        actions.Controls.AddRange(new Control[] { generate, print, refresh }); editor.Controls.Add(actions, 1, 2); editor.SetColumnSpan(actions, 3);
        _doctor.SelectedIndexChanged += (_, _) => CalculateTotal(); _room.SelectedIndexChanged += (_, _) => CalculateTotal(); _days.ValueChanged += (_, _) => CalculateTotal(); _medicine.ValueChanged += (_, _) => CalculateTotal();
        root.Controls.Add(editor); root.Controls.Add(UIHelpers.Header("Billing & Invoice Generator")); Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel table, string label, Control control, int column, int row)
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) }; panel.Controls.Add(control); panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }); table.Controls.Add(panel, column, row);
    }

    private void LoadSelectors()
    {
        try
        {
            var patients = _patients.GetAll().ToList(); _patient.DataSource = patients; _patient.DisplayMember = "FullName"; _patient.ValueMember = "ID";
            var doctors = _doctors.GetAll().ToList(); _doctor.DataSource = doctors; _doctor.DisplayMember = "FullName"; _doctor.ValueMember = "ID";
            var rooms = _rooms.GetAll("Available").ToList(); _room.DataSource = rooms; _room.DisplayMember = "RoomNumber"; _room.ValueMember = "RoomID";
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

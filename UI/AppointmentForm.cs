using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;
using HospitalManagementSystem.Core.Services;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.UI;

public sealed class AppointmentForm : Form
{
    private readonly DBHelper _db = new();
    private readonly PatientRepository _patients;
    private readonly DoctorRepository _doctors;
    private readonly AppointmentService _service;
    private readonly DataGridView _grid = UIHelpers.Grid();
    private readonly ComboBox _patient = UIHelpers.ComboBox();
    private readonly ComboBox _doctor = UIHelpers.ComboBox();
    private readonly DateTimePicker _date = new() { Width = 160, Format = DateTimePickerFormat.Short, MinDate = DateTime.Today };
    private readonly ComboBox _time = UIHelpers.ComboBox();
    private readonly Label _fee = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
    private readonly TextBox _diagnosis = UIHelpers.TextBox();
    private readonly ComboBox _status = UIHelpers.ComboBox();

    public AppointmentForm()
    {
        _patients = new PatientRepository(_db); _doctors = new DoctorRepository(_db); _service = new AppointmentService(new AppointmentRepository(_db), _patients);
        _service.OnAppointmentBooked += EventLogger.LogAppointment;
        Text = "Appointments";
        InitializeCustomComponents(); LoadSelectors(); LoadAppointments();
    }

    private void InitializeCustomComponents()
    {
        var root = new Panel { Dock = DockStyle.Fill }; root.Controls.Add(_grid);
        var editor = new TableLayoutPanel { Dock = DockStyle.Top, Height = 210, ColumnCount = 4, RowCount = 4, Padding = new Padding(8) };
        for (var i = 0; i < 4; i++) editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        AddField(editor, "Patient", _patient, 0, 0); AddField(editor, "Doctor", _doctor, 1, 0); AddField(editor, "Date", _date, 2, 0); AddField(editor, "Time", _time, 3, 0);
        AddField(editor, "Consultation fee", _fee, 0, 1); AddField(editor, "Diagnosis", _diagnosis, 1, 1); AddField(editor, "Status", _status, 2, 1);
        _status.Items.AddRange(new[] { "Scheduled", "Completed", "Cancelled" }); _status.SelectedIndex = 0;
        for (var h = 8; h <= 17; h++) { _time.Items.Add($"{h:00}:00"); _time.Items.Add($"{h:00}:30"); } _time.SelectedIndex = 0;
        _patient.SelectedIndexChanged += (_, _) => { };
        _doctor.SelectedIndexChanged += (_, _) => UpdateFee();
        _date.ValueChanged += (_, _) => LoadAppointments();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        var book = UIHelpers.ActionButton("Book Appointment"); book.Click += (_, _) => Book();
        var refresh = UIHelpers.ActionButton("Refresh"); refresh.Click += (_, _) => { LoadSelectors(); LoadAppointments(); };
        var update = UIHelpers.ActionButton("Update Status"); update.Click += (_, _) => UpdateStatus();
        actions.Controls.AddRange(new Control[] { book, update, refresh }); editor.Controls.Add(actions, 3, 1);
        root.Controls.Add(editor); root.Controls.Add(UIHelpers.Header("Appointment Scheduler")); Controls.Add(root);
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
            var doctors = _doctors.GetAll().ToList();
            var doctorId = SessionManager.Current?.DoctorID;
            if (SessionManager.Current?.Role == "Doctor" && doctorId.HasValue) doctors = doctors.Where(d => d.ID == doctorId.Value).ToList();
            _doctor.DataSource = doctors; _doctor.DisplayMember = "FullName"; _doctor.ValueMember = "ID"; UpdateFee();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Appointments", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void UpdateFee()
    {
        if (_doctor.SelectedItem is Doctor doctor) _fee.Text = $"{doctor.ConsultationFee:0.00}";
        else _fee.Text = "0.00";
    }

    private void LoadAppointments()
    {
        try
        {
            var doctorId = SessionManager.Current?.Role == "Doctor" ? SessionManager.Current.DoctorID : null;
            _grid.DataSource = _service.GetAppointments(doctorId).Select(a => new { a.AppointmentID, Patient = a.PatientName, Doctor = a.DoctorName, Date = a.AppointmentDate.ToString("yyyy-MM-dd HH:mm"), a.Status, a.Diagnosis }).ToList();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Appointments", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private DateTime SelectedDateTime()
    {
        if (_time.SelectedItem is null) throw new InvalidOperationException("Select an appointment time.");
        return _date.Value.Date.Add(TimeSpan.Parse(_time.SelectedItem.ToString()!));
    }

    private void Book()
    {
        try
        {
            if (_patient.SelectedValue is null || _doctor.SelectedValue is null) throw new InvalidOperationException("Select both a patient and a doctor.");
            var appointment = new Appointment { PatientID = Convert.ToInt32(_patient.SelectedValue), DoctorID = Convert.ToInt32(_doctor.SelectedValue), AppointmentDate = SelectedDateTime(), Diagnosis = _diagnosis.Text.Trim() };
            _service.Book(appointment); MessageBox.Show($"Appointment #{appointment.AppointmentID} booked successfully.", "Appointments", MessageBoxButtons.OK, MessageBoxIcon.Information); LoadAppointments();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Appointments", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void UpdateStatus()
    {
        if (_grid.CurrentRow is null) { MessageBox.Show("Select an appointment row first."); return; }
        try
        {
            var id = Convert.ToInt32(_grid.CurrentRow.Cells["AppointmentID"].Value);
            if (_service.UpdateStatus(id, _status.Text, _diagnosis.Text.Trim())) LoadAppointments();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Appointments", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}

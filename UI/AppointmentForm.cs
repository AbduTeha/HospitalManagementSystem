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
        var root = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 248, 248) };

        var editor = new Panel
        {
            Dock = DockStyle.Top,
            Height = 270,
            BackColor = Color.White,
            Padding = new Padding(12),
            BorderStyle = BorderStyle.FixedSingle
        };

        var formTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 4,
            Padding = new Padding(0)
        };

        for (var i = 0; i < 4; i++)
            formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        AddField(formTable, "Patient *", _patient, 0, 0);
        AddField(formTable, "Doctor *", _doctor, 1, 0);
        AddField(formTable, "Date *", _date, 2, 0);
        AddField(formTable, "Time *", _time, 3, 0);

        AddField(formTable, "Consultation Fee", _fee, 0, 1);
        AddField(formTable, "Diagnosis", _diagnosis, 1, 1);
        AddField(formTable, "Status", _status, 2, 1);

        _status.Items.AddRange(new[] { "Scheduled", "Completed", "Cancelled" });
        _status.SelectedIndex = 0;

        for (var h = 8; h <= 17; h++)
        {
            _time.Items.Add($"{h:00}:00");
            _time.Items.Add($"{h:00}:30");
        }
        _time.SelectedIndex = 0;

        _patient.SelectedIndexChanged += (_, _) => { };
        _doctor.SelectedIndexChanged += (_, _) => UpdateFee();
        _date.ValueChanged += (_, _) => LoadAppointments();

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(6),
            BackColor = Color.White,
            AutoSize = true
        };

        var book = UIHelpers.SuccessButton("📅 Book Appointment");
        book.Click += (_, _) => Book();
        var updateStatus = UIHelpers.ActionButton("✏️ Update Status");
        updateStatus.Click += (_, _) => UpdateStatus();
        var refresh = UIHelpers.ActionButton("🔄 Refresh");
        refresh.Click += (_, _) => { LoadSelectors(); LoadAppointments(); };

        buttonPanel.Controls.AddRange(new Control[] { book, updateStatus, refresh });
        formTable.Controls.Add(buttonPanel, 3, 1);

        editor.Controls.Add(formTable);

        var headerSection = new Panel { Dock = DockStyle.Fill, Height = 55, BackColor = Color.White, Padding = new Padding(0) };
        headerSection.Controls.Add(UIHelpers.Header("Appointment Scheduler"));

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
            var doctorId = SessionManager.Current?.DoctorID;
            if (SessionManager.Current?.Role == "Doctor" && doctorId.HasValue)
                doctors = doctors.Where(d => d.ID == doctorId.Value).ToList();

            if (doctors.Count > 0)
            {
                _doctor.DataSource = new BindingSource(doctors, null);
                _doctor.DisplayMember = "FullName";
                _doctor.ValueMember = "ID";
                UpdateFee();
            }
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

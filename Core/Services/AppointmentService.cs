using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;
using HospitalManagementSystem.Core.Security;

namespace HospitalManagementSystem.Core.Services;

public sealed class AppointmentService
{
    private readonly AppointmentRepository _repository;
    private readonly PatientRepository _patients;

    public event Action<Appointment>? OnAppointmentBooked;

    public AppointmentService(AppointmentRepository repository, PatientRepository patients)
    {
        _repository = repository;
        _patients = patients;
    }

    public IReadOnlyList<Appointment> GetAppointments(int? doctorId = null) => _repository.GetAll(doctorId);

    public int Book(Appointment appointment)
    {
        Permissions.Require(Permission.BookAppointment);

        var session = SessionManager.Current;
        if (session?.Role == "Doctor" && appointment.DoctorID != session.DoctorID)
            throw new AccessDeniedException("Doctors can only book appointments for themselves.");

        if (_patients.GetById(appointment.PatientID) is null) throw new PatientNotFoundException(appointment.PatientID);
        if (appointment.DoctorID <= 0) throw new InvalidAppointmentException("A doctor must be selected.");
        if (appointment.AppointmentDate.Minute % 30 != 0) throw new InvalidAppointmentException("Appointments must start on a 30-minute boundary.");

        var id = _repository.Add(appointment);
        appointment.AppointmentID = id;
        OnAppointmentBooked?.Invoke(appointment);
        return id;
    }

    public bool UpdateStatus(int id, string status, string? diagnosis)
    {
        Permissions.Require(Permission.BookAppointment);

        var existing = _repository.GetById(id)
            ?? throw new InvalidAppointmentException("Appointment was not found.");

        var session = SessionManager.Current;
        if (session?.Role == "Doctor" && existing.DoctorID != session.DoctorID)
            throw new AccessDeniedException("Doctors can only change their own appointments.");

        var hasDiagnosis = !string.IsNullOrWhiteSpace(diagnosis);
        if ((hasDiagnosis || status == "Completed") && !Permissions.Has(Permission.WriteDiagnosis))
            throw new AccessDeniedException("Only doctors can record a diagnosis or complete an appointment.");

        // Keep the existing diagnosis when the box is empty. This also fixes the bug
        // where a status change erased the stored diagnosis.
        var newDiagnosis = hasDiagnosis ? diagnosis!.Trim() : existing.Diagnosis;
        return _repository.UpdateStatus(id, status, newDiagnosis);
    }
}
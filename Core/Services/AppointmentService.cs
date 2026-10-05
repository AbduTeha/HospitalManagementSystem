using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Repositories;

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
        if (_patients.GetById(appointment.PatientID) is null) throw new PatientNotFoundException(appointment.PatientID);
        if (appointment.DoctorID <= 0) throw new InvalidAppointmentException("A doctor must be selected.");
        if (appointment.AppointmentDate.Minute % 30 != 0) throw new InvalidAppointmentException("Appointments must start on a 30-minute boundary.");
        var id = _repository.Add(appointment);
        appointment.AppointmentID = id;
        OnAppointmentBooked?.Invoke(appointment);
        return id;
    }

    public bool UpdateStatus(int id, string status, string? diagnosis) => _repository.UpdateStatus(id, status, diagnosis);
}

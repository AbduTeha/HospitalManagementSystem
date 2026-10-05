using HospitalManagementSystem.Core.Models;

namespace HospitalManagementSystem.Core.Interfaces;

public interface IAppointmentRepository
{
    IReadOnlyList<Appointment> GetAll(int? doctorId = null);
    bool HasConflict(int doctorId, DateTime appointmentDate, int? excludeAppointmentId = null);
    int Add(Appointment appointment);
    bool UpdateStatus(int appointmentId, string status, string? diagnosis);
}

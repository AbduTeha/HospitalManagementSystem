namespace HospitalManagementSystem.Core.Models;

public sealed class Appointment
{
    public int AppointmentID { get; set; }
    public int PatientID { get; set; }
    public int DoctorID { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Diagnosis { get; set; }
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
}

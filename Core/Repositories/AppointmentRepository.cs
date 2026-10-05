using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Interfaces;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly DBHelper _db;
    public AppointmentRepository(DBHelper db) => _db = db;

    public IReadOnlyList<Appointment> GetAll(int? doctorId = null)
    {
        const string sql = @"SELECT a.AppointmentID, a.PatientID, a.DoctorID, a.AppointmentDate, a.Status, a.Diagnosis,
                             p.FullName AS PatientName, d.FullName AS DoctorName
                             FROM dbo.Appointments a
                             INNER JOIN dbo.Patients p ON p.PatientID=a.PatientID
                             INNER JOIN dbo.Doctors d ON d.DoctorID=a.DoctorID
                             WHERE (@DoctorID IS NULL OR a.DoctorID=@DoctorID)
                             ORDER BY a.AppointmentDate DESC;";
        var table = _db.ExecuteQuery(sql, new SqlParameter("@DoctorID", SqlDbType.Int) { Value = (object?)doctorId ?? DBNull.Value });
        return table.AsEnumerable().Select(r => new Appointment
        {
            AppointmentID = Convert.ToInt32(r["AppointmentID"]), PatientID = Convert.ToInt32(r["PatientID"]),
            DoctorID = Convert.ToInt32(r["DoctorID"]), AppointmentDate = Convert.ToDateTime(r["AppointmentDate"]),
            Status = Convert.ToString(r["Status"])!, Diagnosis = r["Diagnosis"] == DBNull.Value ? null : Convert.ToString(r["Diagnosis"]),
            PatientName = Convert.ToString(r["PatientName"]), DoctorName = Convert.ToString(r["DoctorName"])
        }).ToList();
    }

    public bool HasConflict(int doctorId, DateTime appointmentDate, int? excludeAppointmentId = null)
    {
        const string sql = @"SELECT COUNT(1) FROM dbo.Appointments
                             WHERE DoctorID=@DoctorID AND Status='Scheduled'
                             AND AppointmentDate >= @Start AND AppointmentDate < @End
                             AND (@ExcludeID IS NULL OR AppointmentID<>@ExcludeID);";
        var count = _db.ExecuteScalar(sql,
            new SqlParameter("@DoctorID", SqlDbType.Int) { Value = doctorId },
            new SqlParameter("@Start", SqlDbType.DateTime2) { Value = appointmentDate },
            new SqlParameter("@End", SqlDbType.DateTime2) { Value = appointmentDate.AddMinutes(30) },
            new SqlParameter("@ExcludeID", SqlDbType.Int) { Value = (object?)excludeAppointmentId ?? DBNull.Value });
        return Convert.ToInt32(count) > 0;
    }

    public int Add(Appointment appointment)
    {
        if (appointment.AppointmentDate <= DateTime.Now)
            throw new InvalidAppointmentException("Appointment must be scheduled in the future.");

        return _db.ExecuteTransaction((connection, transaction) =>
        {
            const string conflictSql = @"SELECT COUNT(1) FROM dbo.Appointments WITH (UPDLOCK, HOLDLOCK)
                                         WHERE DoctorID=@DoctorID AND Status='Scheduled'
                                         AND AppointmentDate >= @Start AND AppointmentDate < @End;";
            using var conflict = new SqlCommand(conflictSql, connection, transaction);
            conflict.Parameters.Add(new SqlParameter("@DoctorID", SqlDbType.Int) { Value = appointment.DoctorID });
            conflict.Parameters.Add(new SqlParameter("@Start", SqlDbType.DateTime2) { Value = appointment.AppointmentDate });
            conflict.Parameters.Add(new SqlParameter("@End", SqlDbType.DateTime2) { Value = appointment.AppointmentDate.AddMinutes(30) });
            if (Convert.ToInt32(conflict.ExecuteScalar()) > 0) throw new DoctorUnavailableException(appointment.AppointmentDate);

            const string insertSql = @"INSERT INTO dbo.Appointments (PatientID, DoctorID, AppointmentDate, Status, Diagnosis)
                                       VALUES (@PatientID,@DoctorID,@AppointmentDate,'Scheduled',@Diagnosis);
                                       SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var insert = new SqlCommand(insertSql, connection, transaction);
            insert.Parameters.Add(new SqlParameter("@PatientID", SqlDbType.Int) { Value = appointment.PatientID });
            insert.Parameters.Add(new SqlParameter("@DoctorID", SqlDbType.Int) { Value = appointment.DoctorID });
            insert.Parameters.Add(new SqlParameter("@AppointmentDate", SqlDbType.DateTime2) { Value = appointment.AppointmentDate });
            insert.Parameters.Add(new SqlParameter("@Diagnosis", SqlDbType.VarChar, 500) { Value = (object?)appointment.Diagnosis ?? DBNull.Value });
            return Convert.ToInt32(insert.ExecuteScalar());
        });
    }

    public bool UpdateStatus(int appointmentId, string status, string? diagnosis)
    {
        if (!new[] { "Scheduled", "Completed", "Cancelled" }.Contains(status))
            throw new InvalidAppointmentException("Invalid appointment status.");
        const string sql = "UPDATE dbo.Appointments SET Status=@Status, Diagnosis=@Diagnosis WHERE AppointmentID=@ID;";
        return _db.ExecuteNonQuery(sql,
            new SqlParameter("@ID", SqlDbType.Int) { Value = appointmentId },
            new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = status },
            new SqlParameter("@Diagnosis", SqlDbType.VarChar, 500) { Value = (object?)diagnosis ?? DBNull.Value }) > 0;
    }
}

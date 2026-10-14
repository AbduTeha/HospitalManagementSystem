using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Interfaces;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Security;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly DBHelper _db;
    public DoctorRepository(DBHelper db) => _db = db;

    public IReadOnlyList<Doctor> GetAll()
    {
        const string sql = "SELECT DoctorID, FullName, Specialization, Phone, Email, ConsultationFee FROM dbo.Doctors ORDER BY FullName;";
        return _db.ExecuteQuery(sql).AsEnumerable().Select(Map).ToList();
    }

    public Doctor? GetById(int id)
    {
        const string sql = "SELECT DoctorID, FullName, Specialization, Phone, Email, ConsultationFee FROM dbo.Doctors WHERE DoctorID=@ID;";
        var table = _db.ExecuteQuery(sql, new SqlParameter("@ID", SqlDbType.Int) { Value = id });
        return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
    }

    public int Add(Doctor doctor)
    {
        Permissions.Require(Permission.ManageDoctors);

        const string sql = @"INSERT INTO dbo.Doctors (FullName, Specialization, Phone, Email, ConsultationFee)
                             VALUES (@FullName,@Specialization,@Phone,@Email,@Fee);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        var result = _db.ExecuteScalar(sql,
            new SqlParameter("@FullName", SqlDbType.VarChar, 100) { Value = doctor.FullName },
            new SqlParameter("@Specialization", SqlDbType.VarChar, 100) { Value = doctor.Specialization },
            new SqlParameter("@Phone", SqlDbType.VarChar, 20) { Value = doctor.Phone },
            new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = (object?)doctor.Email ?? DBNull.Value },
            new SqlParameter("@Fee", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = doctor.ConsultationFee });
        return Convert.ToInt32(result);
    }

    public bool Update(Doctor doctor)
    {
        Permissions.Require(Permission.ManageDoctors);

        const string sql = @"UPDATE dbo.Doctors SET FullName=@FullName, Specialization=@Specialization, Phone=@Phone,
                             Email=@Email, ConsultationFee=@Fee WHERE DoctorID=@ID;";
        return _db.ExecuteNonQuery(sql,
            new SqlParameter("@ID", SqlDbType.Int) { Value = doctor.ID },
            new SqlParameter("@FullName", SqlDbType.VarChar, 100) { Value = doctor.FullName },
            new SqlParameter("@Specialization", SqlDbType.VarChar, 100) { Value = doctor.Specialization },
            new SqlParameter("@Phone", SqlDbType.VarChar, 20) { Value = doctor.Phone },
            new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = (object?)doctor.Email ?? DBNull.Value },
            new SqlParameter("@Fee", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = doctor.ConsultationFee }) > 0;
    }

    public bool Delete(int id)
    {
        Permissions.Require(Permission.ManageDoctors);

        const string sql = "DELETE FROM dbo.Doctors WHERE DoctorID=@ID;";
        return _db.ExecuteNonQuery(sql, new SqlParameter("@ID", SqlDbType.Int) { Value = id }) > 0;
    }

    private static Doctor Map(DataRow row) => new()
    {
        ID = Convert.ToInt32(row["DoctorID"]),
        FullName = Convert.ToString(row["FullName"])!,
        Specialization = Convert.ToString(row["Specialization"])!,
        Phone = Convert.ToString(row["Phone"])!,
        Email = row["Email"] == DBNull.Value ? null : Convert.ToString(row["Email"]),
        ConsultationFee = Convert.ToDecimal(row["ConsultationFee"])
    };
}
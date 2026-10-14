using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Interfaces;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Security;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly DBHelper _db;
    public PatientRepository(DBHelper db) => _db = db;

    public IReadOnlyList<Patient> GetAll(string? search = null)
    {
        const string sql = @"SELECT PatientID, FullName, Gender, DOB, Phone, Address, BloodGroup, RegisteredAt
                             FROM dbo.Patients
                             WHERE (@Search = '' OR FullName LIKE '%' + @Search + '%' OR Phone LIKE '%' + @Search + '%')
                             ORDER BY PatientID DESC;";
        var table = _db.ExecuteQuery(sql, new SqlParameter("@Search", SqlDbType.VarChar, 100) { Value = search?.Trim() ?? string.Empty });
        return table.AsEnumerable().Select(Map).ToList();
    }

    public Patient? GetById(int id)
    {
        const string sql = "SELECT PatientID, FullName, Gender, DOB, Phone, Address, BloodGroup, RegisteredAt FROM dbo.Patients WHERE PatientID=@ID;";
        var table = _db.ExecuteQuery(sql, new SqlParameter("@ID", SqlDbType.Int) { Value = id });
        return table.Rows.Count == 0 ? null : Map(table.Rows[0]);
    }

    public int Add(Patient patient)
    {
        Permissions.Require(Permission.ManagePatients);

        const string sql = @"INSERT INTO dbo.Patients (FullName, Gender, DOB, Phone, Address, BloodGroup)
                             VALUES (@FullName,@Gender,@DOB,@Phone,@Address,@BloodGroup);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        var result = _db.ExecuteScalar(sql,
            new SqlParameter("@FullName", SqlDbType.VarChar, 100) { Value = patient.FullName },
            new SqlParameter("@Gender", SqlDbType.VarChar, 10) { Value = patient.Gender },
            new SqlParameter("@DOB", SqlDbType.Date) { Value = patient.DOB },
            new SqlParameter("@Phone", SqlDbType.VarChar, 20) { Value = patient.Phone },
            new SqlParameter("@Address", SqlDbType.VarChar, 255) { Value = patient.Address },
            new SqlParameter("@BloodGroup", SqlDbType.VarChar, 5) { Value = patient.BloodGroup });
        return Convert.ToInt32(result);
    }

    public bool Update(Patient patient)
    {
        Permissions.Require(Permission.ManagePatients);

        const string sql = @"UPDATE dbo.Patients SET FullName=@FullName, Gender=@Gender, DOB=@DOB, Phone=@Phone,
                             Address=@Address, BloodGroup=@BloodGroup WHERE PatientID=@ID;";
        return _db.ExecuteNonQuery(sql,
            new SqlParameter("@ID", SqlDbType.Int) { Value = patient.ID },
            new SqlParameter("@FullName", SqlDbType.VarChar, 100) { Value = patient.FullName },
            new SqlParameter("@Gender", SqlDbType.VarChar, 10) { Value = patient.Gender },
            new SqlParameter("@DOB", SqlDbType.Date) { Value = patient.DOB },
            new SqlParameter("@Phone", SqlDbType.VarChar, 20) { Value = patient.Phone },
            new SqlParameter("@Address", SqlDbType.VarChar, 255) { Value = patient.Address },
            new SqlParameter("@BloodGroup", SqlDbType.VarChar, 5) { Value = patient.BloodGroup }) > 0;
    }

    public bool Delete(int id)
    {
        Permissions.Require(Permission.DeletePatient);

        if (GetById(id) is null) throw new PatientNotFoundException(id);
        const string sql = "DELETE FROM dbo.Patients WHERE PatientID=@ID;";
        return _db.ExecuteNonQuery(sql, new SqlParameter("@ID", SqlDbType.Int) { Value = id }) > 0;
    }

    private static Patient Map(DataRow row) => new()
    {
        ID = Convert.ToInt32(row["PatientID"]),
        FullName = Convert.ToString(row["FullName"])!,
        Gender = Convert.ToString(row["Gender"])!,
        DOB = Convert.ToDateTime(row["DOB"]),
        Phone = Convert.ToString(row["Phone"])!,
        Address = Convert.ToString(row["Address"])!,
        BloodGroup = Convert.ToString(row["BloodGroup"])!,
        RegisteredAt = Convert.ToDateTime(row["RegisteredAt"])
    };
}
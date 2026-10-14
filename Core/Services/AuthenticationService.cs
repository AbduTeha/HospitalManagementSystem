using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Services;

public sealed class AuthenticationService
{
    private readonly DBHelper _db;
    public AuthenticationService(DBHelper db) => _db = db;

    public UserSession? Authenticate(string username, string password)
    {
        const string sql = @"SELECT u.UserID, u.Username, u.PasswordHash, u.FullName, u.Role, u.DoctorID
                             FROM dbo.Users u
                             WHERE u.Username=@Username;";
        var table = _db.ExecuteQuery(sql, new SqlParameter("@Username", SqlDbType.VarChar, 50) { Value = username.Trim() });
        if (table.Rows.Count == 0) return null;
        var row = table.Rows[0];
        if (!PasswordHasher.Verify(password, Convert.ToString(row["PasswordHash"])!)) return null;
        return new UserSession
        {
            UserID = Convert.ToInt32(row["UserID"]),
            Username = Convert.ToString(row["Username"])!,
            FullName = Convert.ToString(row["FullName"])!,
            Role = Convert.ToString(row["Role"])!,
            DoctorID = row["DoctorID"] == DBNull.Value ? null : Convert.ToInt32(row["DoctorID"])
        };
    }
}
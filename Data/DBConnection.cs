using Microsoft.Data.SqlClient;

namespace HospitalManagementSystem.Data;

public static class DBConnection
{
    private static readonly string DefaultConnectionString =
        "Server=localhost;Database=HospitalDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=False;";

    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("HOSPITAL_DB_CONNECTION") ?? DefaultConnectionString;

    public static SqlConnection CreateConnection() => new(ConnectionString);
}

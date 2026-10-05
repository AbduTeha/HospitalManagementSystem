namespace HospitalManagementSystem.Core.Services;

public sealed class UserSession
{
    public int UserID { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public int? DoctorID { get; init; }
}

public static class SessionManager
{
    public static UserSession? Current { get; private set; }
    public static void Start(UserSession session) => Current = session;
    public static void End() => Current = null;
}

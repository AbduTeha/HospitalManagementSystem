using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Services;

namespace HospitalManagementSystem.Core.Security;

public enum Permission
{
    ManageUsers,
    ManageDoctors,
    ManageRooms,
    ManagePatients,
    DeletePatient,
    BookAppointment,
    WriteDiagnosis,
    ManageBilling,
    ViewAuditLog
}

public static class Permissions
{
    private static readonly Dictionary<string, HashSet<Permission>> RolePermissions = new()
    {
        ["Admin"] = Enum.GetValues<Permission>().ToHashSet(),
        ["Doctor"] = new HashSet<Permission>
        {
            Permission.ManagePatients,
            Permission.BookAppointment,
            Permission.WriteDiagnosis
        },
        ["Receptionist"] = new HashSet<Permission>
        {
            Permission.ManagePatients,
            Permission.BookAppointment,
            Permission.ManageRooms,
            Permission.ManageBilling
        }
    };

    public static bool Has(Permission permission)
    {
        var session = SessionManager.Current;
        return session is not null
            && RolePermissions.TryGetValue(session.Role, out var allowed)
            && allowed.Contains(permission);
    }

    public static void Require(Permission permission)
    {
        if (!Has(permission))
            throw new AccessDeniedException($"Your role does not allow this action ({permission}).");
    }
}
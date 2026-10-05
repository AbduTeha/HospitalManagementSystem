namespace HospitalManagementSystem.Core.Models;

public sealed class Staff : Person
{
    public string Role { get; set; } = "Receptionist";

    public override string GetDisplayDetails() => $"Staff #{ID}: {FullName} | {Role} | Phone {Phone}";
}

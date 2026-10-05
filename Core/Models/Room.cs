namespace HospitalManagementSystem.Core.Models;

public sealed class Room
{
    public int RoomID { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = "General";
    public decimal DailyRate { get; set; }
    public string Status { get; set; } = "Available";
}

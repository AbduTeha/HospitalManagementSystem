namespace HospitalManagementSystem.Core.Models;

public sealed class Bill
{
    public int BillID { get; set; }
    public int PatientID { get; set; }
    public string? PatientName { get; set; }
    public decimal DoctorFee { get; set; }
    public decimal RoomFee { get; set; }
    public decimal MedicineFee { get; set; }
    public decimal TotalAmount => DoctorFee + RoomFee + MedicineFee;
    public string PaymentStatus { get; set; } = "Unpaid";
    public DateTime BillDate { get; set; }
}

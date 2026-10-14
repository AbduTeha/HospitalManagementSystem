using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Security;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Services;

public sealed class BillingService
{
    private readonly DBHelper _db;
    public event Action<Bill>? OnBillGenerated;

    public BillingService(DBHelper db) => _db = db;

    public int GenerateBill(Bill bill)
    {
        Permissions.Require(Permission.ManageBilling);

        if (bill.PatientID <= 0) throw new PatientNotFoundException(bill.PatientID);
        if (bill.DoctorFee < 0 || bill.RoomFee < 0 || bill.MedicineFee < 0)
            throw new ArgumentException("Bill charges cannot be negative.");

        var id = _db.ExecuteTransaction((connection, transaction) =>
        {
            const string patientSql = "SELECT COUNT(1) FROM dbo.Patients WITH (UPDLOCK, HOLDLOCK) WHERE PatientID=@PatientID;";
            using var patient = new SqlCommand(patientSql, connection, transaction);
            patient.Parameters.Add(new SqlParameter("@PatientID", SqlDbType.Int) { Value = bill.PatientID });
            if (Convert.ToInt32(patient.ExecuteScalar()) == 0) throw new PatientNotFoundException(bill.PatientID);

            const string sql = @"INSERT INTO dbo.Billings (PatientID, DoctorFee, RoomFee, MedicineFee, PaymentStatus)
                                 VALUES (@PatientID,@DoctorFee,@RoomFee,@MedicineFee,@PaymentStatus);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.Add(new SqlParameter("@PatientID", SqlDbType.Int) { Value = bill.PatientID });
            command.Parameters.Add(new SqlParameter("@DoctorFee", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = bill.DoctorFee });
            command.Parameters.Add(new SqlParameter("@RoomFee", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = bill.RoomFee });
            command.Parameters.Add(new SqlParameter("@MedicineFee", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = bill.MedicineFee });
            command.Parameters.Add(new SqlParameter("@PaymentStatus", SqlDbType.VarChar, 20) { Value = bill.PaymentStatus });
            return Convert.ToInt32(command.ExecuteScalar());
        });

        bill.BillID = id;
        bill.BillDate = DateTime.Now;
        OnBillGenerated?.Invoke(bill);
        return id;
    }

    public IReadOnlyList<Bill> GetAll()
    {
        const string sql = @"SELECT b.BillID,b.PatientID,p.FullName AS PatientName,b.DoctorFee,b.RoomFee,b.MedicineFee,
                             b.TotalAmount,b.PaymentStatus,b.BillDate
                             FROM dbo.Billings b INNER JOIN dbo.Patients p ON p.PatientID=b.PatientID
                             ORDER BY b.BillDate DESC;";
        return _db.ExecuteQuery(sql).AsEnumerable().Select(r => new Bill
        {
            BillID = Convert.ToInt32(r["BillID"]),
            PatientID = Convert.ToInt32(r["PatientID"]),
            PatientName = Convert.ToString(r["PatientName"]),
            DoctorFee = Convert.ToDecimal(r["DoctorFee"]),
            RoomFee = Convert.ToDecimal(r["RoomFee"]),
            MedicineFee = Convert.ToDecimal(r["MedicineFee"]),
            PaymentStatus = Convert.ToString(r["PaymentStatus"])!,
            BillDate = Convert.ToDateTime(r["BillDate"])
        }).ToList();
    }

    public bool SetPaymentStatus(int billId, string status)
    {
        Permissions.Require(Permission.ManageBilling);

        if (!new[] { "Paid", "Unpaid" }.Contains(status)) throw new ArgumentException("Invalid payment status.");
        const string sql = "UPDATE dbo.Billings SET PaymentStatus=@Status WHERE BillID=@ID;";
        return _db.ExecuteNonQuery(sql,
            new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = status },
            new SqlParameter("@ID", SqlDbType.Int) { Value = billId }) > 0;
    }
}
using System.Data;
using Microsoft.Data.SqlClient;
using HospitalManagementSystem.Core.Exceptions;
using HospitalManagementSystem.Core.Models;
using HospitalManagementSystem.Core.Security;
using HospitalManagementSystem.Data;

namespace HospitalManagementSystem.Core.Services;

public sealed class RoomService
{
    private readonly DBHelper _db;
    public RoomService(DBHelper db) => _db = db;

    public IReadOnlyList<Room> GetAll(string? status = null)
    {
        const string sql = "SELECT RoomID,RoomNumber,RoomType,DailyRate,Status FROM dbo.Rooms WHERE (@Status='' OR Status=@Status) ORDER BY RoomNumber;";
        return _db.ExecuteQuery(sql, new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = status ?? string.Empty })
            .AsEnumerable().Select(r => new Room { RoomID = Convert.ToInt32(r["RoomID"]), RoomNumber = Convert.ToString(r["RoomNumber"])!, RoomType = Convert.ToString(r["RoomType"])!, DailyRate = Convert.ToDecimal(r["DailyRate"]), Status = Convert.ToString(r["Status"])! }).ToList();
    }

    public int Add(Room room)
    {
        Permissions.Require(Permission.ManageRooms);

        if (room.DailyRate < 0) throw new ArgumentException("Daily rate cannot be negative.");
        const string sql = @"INSERT INTO dbo.Rooms(RoomNumber,RoomType,DailyRate,Status) VALUES(@Number,@Type,@Rate,@Status); SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return Convert.ToInt32(_db.ExecuteScalar(sql,
            new SqlParameter("@Number", SqlDbType.VarChar, 20) { Value = room.RoomNumber.Trim() },
            new SqlParameter("@Type", SqlDbType.VarChar, 50) { Value = room.RoomType },
            new SqlParameter("@Rate", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = room.DailyRate },
            new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = room.Status }));
    }

    public bool SetStatus(int roomId, string status)
    {
        Permissions.Require(Permission.ManageRooms);

        if (!new[] { "Available", "Occupied" }.Contains(status)) throw new ArgumentException("Invalid room status.");
        if (status == "Occupied")
        {
            const string check = "SELECT Status FROM dbo.Rooms WHERE RoomID=@ID;";
            var current = _db.ExecuteScalar(check, new SqlParameter("@ID", SqlDbType.Int) { Value = roomId }) as string;
            if (string.Equals(current, "Occupied", StringComparison.OrdinalIgnoreCase)) throw new RoomOccupiedException(roomId);
        }
        const string sql = "UPDATE dbo.Rooms SET Status=@Status WHERE RoomID=@ID;";
        return _db.ExecuteNonQuery(sql, new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = status }, new SqlParameter("@ID", SqlDbType.Int) { Value = roomId }) > 0;
    }
}
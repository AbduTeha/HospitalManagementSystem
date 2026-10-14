namespace HospitalManagementSystem.Core.Exceptions;

public class HospitalException : Exception
{
    public HospitalException(string message) : base(message) { }
    public HospitalException(string message, Exception innerException) : base(message, innerException) { }
}

public sealed class PatientNotFoundException : HospitalException
{
    public PatientNotFoundException(int patientId) : base($"Patient #{patientId} was not found.") { }
}

public sealed class DoctorUnavailableException : HospitalException
{
    public DoctorUnavailableException(DateTime date) : base($"The selected doctor is already booked for {date:yyyy-MM-dd HH:mm}.") { }
}

public sealed class InvalidAppointmentException : HospitalException
{
    public InvalidAppointmentException(string message) : base(message) { }
}

public sealed class AccessDeniedException : HospitalException
{
    public AccessDeniedException(string message) : base(message) { }
}

public class RoomOccupiedException : HospitalException
{
    public RoomOccupiedException(int roomId) : base($"Room #{roomId} is not available.") { }
}

public sealed class RoomNotAvailableException : RoomOccupiedException
{
    public RoomNotAvailableException(int roomId) : base(roomId) { }
}

public sealed class DatabaseExecutionException : HospitalException
{
    public DatabaseExecutionException(string message, Exception innerException) : base(message, innerException) { }
}
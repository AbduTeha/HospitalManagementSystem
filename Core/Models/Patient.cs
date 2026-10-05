namespace HospitalManagementSystem.Core.Models;

public sealed class Patient : Person
{
    private DateTime _dob;
    private string _gender = "Male";
    private string _address = string.Empty;
    private string _bloodGroup = string.Empty;

    public DateTime DOB
    {
        get => _dob;
        set
        {
            if (value.Date > DateTime.Today) throw new ArgumentException("Date of birth cannot be in the future.");
            _dob = value.Date;
        }
    }

    public string Gender
    {
        get => _gender;
        set
        {
            if (!new[] { "Male", "Female", "Other" }.Contains(value)) throw new ArgumentException("Invalid gender.");
            _gender = value;
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Address is required.");
            if (value.Trim().Length > 255) throw new ArgumentException("Address cannot exceed 255 characters.");
            _address = value.Trim();
        }
    }

    public string BloodGroup
    {
        get => _bloodGroup;
        set
        {
            var valid = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            if (!valid.Contains(value)) throw new ArgumentException("Invalid blood group.");
            _bloodGroup = value;
        }
    }

    public DateTime RegisteredAt { get; set; }

    public override string GetDisplayDetails() =>
        $"Patient #{ID}: {FullName} | {Gender} | DOB {DOB:yyyy-MM-dd} | Phone {Phone} | Blood {BloodGroup}";
}

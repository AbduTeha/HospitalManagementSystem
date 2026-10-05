namespace HospitalManagementSystem.Core.Models;

public abstract class Person
{
    private string _fullName = string.Empty;
    private string _phone = string.Empty;

    public int ID { get; set; }

    public string FullName
    {
        get => _fullName;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Full name is required.");
            if (value.Trim().Length > 100) throw new ArgumentException("Full name cannot exceed 100 characters.");
            _fullName = value.Trim();
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            var cleaned = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cleaned)) throw new ArgumentException("Phone number is required.");
            if (cleaned.Length > 20) throw new ArgumentException("Phone number cannot exceed 20 characters.");
            if (cleaned.Any(c => !char.IsDigit(c) && c != '+' && c != '-' && c != ' ' && c != '(' && c != ')'))
                throw new ArgumentException("Phone number contains invalid characters.");
            _phone = cleaned;
        }
    }

    public abstract string GetDisplayDetails();
}

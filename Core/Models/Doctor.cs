namespace HospitalManagementSystem.Core.Models;

public sealed class Doctor : Person
{
    private string _specialization = string.Empty;
    private string? _email;
    private decimal _consultationFee;

    public string Specialization
    {
        get => _specialization;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Specialization is required.");
            if (value.Trim().Length > 100) throw new ArgumentException("Specialization cannot exceed 100 characters.");
            _specialization = value.Trim();
        }
    }

    public string? Email
    {
        get => _email;
        set
        {
            var valueTrimmed = value?.Trim();
            if (!string.IsNullOrEmpty(valueTrimmed) && (!valueTrimmed.Contains('@') || valueTrimmed.Length > 100))
                throw new ArgumentException("Enter a valid email address.");
            _email = valueTrimmed;
        }
    }

    public decimal ConsultationFee
    {
        get => _consultationFee;
        set
        {
            if (value < 0) throw new ArgumentException("Consultation fee cannot be negative.");
            _consultationFee = decimal.Round(value, 2);
        }
    }

    public override string GetDisplayDetails() =>
        $"Doctor #{ID}: {FullName} | {Specialization} | Phone {Phone} | Fee {ConsultationFee:C}";
}

using HospitalManagementSystem.Core.Models;

namespace HospitalManagementSystem.Core.Interfaces;

public interface IPatientRepository
{
    IReadOnlyList<Patient> GetAll(string? search = null);
    Patient? GetById(int id);
    int Add(Patient patient);
    bool Update(Patient patient);
    bool Delete(int id);
}

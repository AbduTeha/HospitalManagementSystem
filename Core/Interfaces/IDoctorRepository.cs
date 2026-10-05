using HospitalManagementSystem.Core.Models;

namespace HospitalManagementSystem.Core.Interfaces;

public interface IDoctorRepository
{
    IReadOnlyList<Doctor> GetAll();
    Doctor? GetById(int id);
    int Add(Doctor doctor);
    bool Update(Doctor doctor);
    bool Delete(int id);
}

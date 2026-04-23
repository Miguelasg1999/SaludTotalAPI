using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface IPatientRepository: IRepository<Patient>
{
    Task<Patient?> GetCurrentUser(string userId);
    Task<Patient?> GetPatientById(int id);
    Task<Patient?> GetPatientByRut(string rut);
    Task<IEnumerable<Patient>> GetPatients();
}

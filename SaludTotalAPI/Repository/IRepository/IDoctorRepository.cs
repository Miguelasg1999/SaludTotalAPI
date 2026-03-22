using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface IDoctorRepository: IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetFiltered(int? specialtyId, int page, int pageSize);
    Task<bool> EmailExists(string email);
}

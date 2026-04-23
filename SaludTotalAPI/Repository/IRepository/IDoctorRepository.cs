using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface IDoctorRepository: IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetPagedDoctors(int? specialtyId, int page, int pageSize);

    Task<Doctor?> GetDoctorById(int id);
}

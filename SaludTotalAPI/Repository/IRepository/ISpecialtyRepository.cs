using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface ISpecialtyRepository: IRepository<Specialty>
{
    Task<Specialty?> GetWithDoctors(int id);
    bool SpecialtyExists(string name);
}

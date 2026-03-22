using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface ISpecialtyRepository: IRepository<Specialty>
{
    Task<Specialty?> GetWithDoctors(int id);
    Task<bool> SpecialtyExists(string name);
    Task<bool> SpecialtyExists(int id);
    Task<bool> HasDoctor(int id);
}

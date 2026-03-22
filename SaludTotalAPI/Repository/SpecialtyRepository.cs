using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Models;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class SpecialtyRepository : Repository<Specialty>, ISpecialtyRepository
{
    private readonly ApplicationDbContext _db;

    public SpecialtyRepository(ApplicationDbContext db): base(db)
    {
        _db = db;
    }

    public async Task<Specialty?> GetWithDoctors(int id)
    {
        return await _db.Specialties
            .AsNoTracking()
            .Include(s => s.Doctors)
            .FirstOrDefaultAsync(s => s.SpecialtyId == id);
    }

    public async Task<bool> SpecialtyExists(string name)
    {
        return _db.Specialties.Any(s => s.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public async Task<bool> SpecialtyExists(int id)
    {
        return _db.Specialties.Any(s => s.SpecialtyId == id);
    } 

    public Task<bool> HasDoctor(int id)
    {
        return _db.Doctors.AnyAsync(d => d.SpecialtyId == id);
    }

}

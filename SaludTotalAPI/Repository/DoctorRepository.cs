using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Models;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class DoctorRepository: Repository<Doctor> , IDoctorRepository
{
    private readonly ApplicationDbContext _db;
    
    public DoctorRepository(ApplicationDbContext db): base(db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Doctor>> GetFiltered(int? specialtyId, int page, int pageSize)
    {
        var query = _db.Doctors.AsQueryable();

        if (specialtyId.HasValue)
        {
            query = query.Where(d => d.SpecialtyId == specialtyId.Value);
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> EmailExists(string email)
    {
        return await _db.Doctors.AnyAsync(d => d.Email == email);
    }

}

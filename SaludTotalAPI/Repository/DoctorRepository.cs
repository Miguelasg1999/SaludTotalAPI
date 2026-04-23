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

    public async Task<Doctor?> GetDoctorById(int id)
    {
        return await _db.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.DoctorId == id);
    }

    public async Task<IEnumerable<Doctor>> GetPagedDoctors(int? specialtyId, int page, int pageSize)
    {
        var query = _db.Doctors
        .Include(d => d.User)
        .Include(d => d.Specialty)
        .AsNoTracking()
        .AsQueryable();

        if (specialtyId.HasValue)
        {
            query = query.Where(d => d.SpecialtyId == specialtyId.Value);
        }

        return await query
            .OrderBy(d => d.DoctorId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

}

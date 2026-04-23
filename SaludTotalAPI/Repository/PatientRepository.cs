using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Models;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    private readonly ApplicationDbContext _db;
    public PatientRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Patient?> GetCurrentUser(string userId)
    {
        return await _db.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }
    public async Task<Patient?> GetPatientById(int id)
    {
        return await _db.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }
    public async Task<Patient?> GetPatientByRut(string rut)
    {
        return await _db.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.User.Rut == rut);
    }

    public async Task<IEnumerable<Patient>> GetPatients()
    {
        return await _db.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .ToListAsync();
    }

}

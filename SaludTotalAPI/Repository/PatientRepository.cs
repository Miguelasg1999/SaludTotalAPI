using System;
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
}

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Models;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    private readonly ApplicationDbContext _db;
    public MedicalRecordRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<MedicalRecord?> GetByPatientId(int patientId)
    {
        return await _db.MedicalRecords.FirstOrDefaultAsync(m => m.PatientId == patientId);
    }

    public async Task<MedicalRecord?> GetByUserId(string userId)
    {
        return await _db.MedicalRecords
            .Include(m => m.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(m => m.Patient.UserId == userId);
    }
}

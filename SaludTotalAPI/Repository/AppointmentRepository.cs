using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Data;
using SaludTotalAPI.Models;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Repository;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    private readonly ApplicationDbContext _db;

    public AppointmentRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Appointment?> GetAppointmentWithDetails(int id)
    {
        return await _db.Appointments
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDate(int doctorId, DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            var dateChange = startDate;
            startDate = endDate;
            endDate = dateChange;
        }

        return await _db.Appointments
            .Where(a => a.DoctorId == doctorId 
                     && a.AppointmentDateTime >= startDate 
                     && a.AppointmentDateTime <= endDate)
                .ToListAsync();
    }

}

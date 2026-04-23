using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface IAppointmentRepository: IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByDoctorAndDate(int doctorId, DateTime startDate, DateTime endDate);
    Task<Appointment?> GetAppointmentWithDetails(int id);
}

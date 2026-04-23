using System;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Repository.IRepository;

public interface IMedicalRecordRepository: IRepository<MedicalRecord>
{
    Task<MedicalRecord?> GetByPatientId(int patientId);
    Task<MedicalRecord?> GetByUserId(string userId);
}

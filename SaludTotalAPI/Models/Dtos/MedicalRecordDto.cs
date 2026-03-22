using System;

namespace SaludTotalAPI.Models.Dtos;

public class MedicalRecordDto
{
    public int MedicalRecordId { get; set; }
    public DateTime CreationDate { get; set; }
    public string? MedicalNotes { get; set; }
    public string? Allergies { get; set; }

    public string? CurrentMedications { get; set; }
    public int PatientId { get; set; }
}

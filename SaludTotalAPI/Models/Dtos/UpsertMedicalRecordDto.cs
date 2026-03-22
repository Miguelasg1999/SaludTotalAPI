using System;

namespace SaludTotalAPI.Models.Dtos;

public class UpsertMedicalRecordDto
{
    public string? MedicalNotes { get; set; }
    public string? Allergies { get; set; }

    public string? CurrentMedications { get; set; }
}

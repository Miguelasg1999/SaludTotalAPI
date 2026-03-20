using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludTotalAPI.Models;

public class MedicalRecord
{
    [Key]
    public int MedicalRecordId { get; set; }
    public DateTime CreationDate { get; set; }
    public string? MedicalNotes { get; set; }
    public string? Allergies { get; set; }

    public string? CurrentMedications { get; set; }
    [ForeignKey("PatientId")]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
}

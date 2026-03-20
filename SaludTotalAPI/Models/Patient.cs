using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models;

public class Patient
{
    [Key]
    public int PatientId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string? Phone { get; set; }
    public DateTime Birthdate { get; set; }
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

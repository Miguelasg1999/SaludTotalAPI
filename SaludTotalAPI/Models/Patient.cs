using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludTotalAPI.Models;

public class Patient
{
    [Key]
    public int PatientId { get; set; }
    public string? Phone { get; set; }
    public DateTime Birthdate { get; set; }
    [ForeignKey("UserId")]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public MedicalRecord MedicalRecord { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

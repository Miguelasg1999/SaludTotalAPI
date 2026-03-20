using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaludTotalAPI.Models;

public class Doctor
{
    [Key]
    public int DoctorId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    [ForeignKey("SpecialtyId")]
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

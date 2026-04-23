using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SaludTotalAPI.Models;

public class Doctor
{
    [Key]
    public int DoctorId { get; set; }
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    [ForeignKey("UserId")]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey("SpecialtyId")]
    public int SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

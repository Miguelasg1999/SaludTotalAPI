using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models;

public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    [ForeignKey("DoctorId")]
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    [ForeignKey("PatientId")]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

}

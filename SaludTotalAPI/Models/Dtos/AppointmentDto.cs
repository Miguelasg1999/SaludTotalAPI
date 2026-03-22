using System;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models.Dtos;

public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
}

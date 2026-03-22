using System;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models.Dtos;

public class CreateAppointmentDto
{
    public DateTime AppointmentDateTime { get; set; }
    public string? Reason { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
}

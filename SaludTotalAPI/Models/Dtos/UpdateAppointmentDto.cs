using System;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models.Dtos;

public class UpdateAppointmentDto
{
    public AppointmentStatus Status { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models.Dtos;

public class UpdateAppointmentDto
{
    [Required(ErrorMessage = "Debe ingresar un estado valido Programada/Confirmada/Cancelada/Completada")]
    public AppointmentStatus Status { get; set; }
}

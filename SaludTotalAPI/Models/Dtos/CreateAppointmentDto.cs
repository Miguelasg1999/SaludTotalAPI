using System;
using System.ComponentModel.DataAnnotations;
using SaludTotalAPI.Enums;

namespace SaludTotalAPI.Models.Dtos;

public class CreateAppointmentDto
{
    [Required(ErrorMessage = "La fecha y hora de la cita es obligatoria")]
    public DateTime AppointmentDateTime { get; set; }
    [StringLength(500, ErrorMessage = "La razón de la cita no puede exceder los 500 caracteres")]
    public string? Reason { get; set; }
    [Required(ErrorMessage = "Debe seleccionar el doctor para la cita")]
    public int DoctorId { get; set; }
    [Required(ErrorMessage = "Debe seleccionar el paciente para la cita")]
    public int PatientId { get; set; }
}

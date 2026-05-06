using Asp.Versioning;
using Ganss.Xss;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SaludTotalAPI.Enums;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    { 

        private readonly IAppointmentRepository _appointmentRepository;

        private readonly IDoctorRepository _doctorRepository;

        private readonly IPatientRepository _patientRepository;

        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository, ILogger<AppointmentsController> logger)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            var appointment = await _appointmentRepository.GetAppointmentWithDetails(id);

            _logger.LogInformation("=== Usuario {UserId} solicitando cita {AppointmentId} ===", userId, id);

            if (appointment == null)
            {
                _logger.LogWarning("=== Cita con id {AppointmentId} no encontrada ===", id);
                return NotFound();
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("=== Usuario autenticado sin NameIdentifier intentando acceder a cita {AppointmentId} ===", id);
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                
                if (User.IsInRole("Patient") && appointment.Patient.UserId != userId)
                {
                    _logger.LogWarning("=== Usuario paciente {UserId} intentando acceder a la cita {AppointmentId} de otro paciente ===", userId, id);
                    return Forbid();
                }

                if (User.IsInRole("Doctor") && appointment.Doctor.UserId != userId)
                {
                    _logger.LogWarning("=== Usuario doctor {UserId} intentando acceder a la cita {AppointmentId} de otro doctor ===", userId, id);
                    return Forbid();
                }
            }

            var appointmentsDto = new
            {
                appointment.AppointmentId,
                appointment.AppointmentDateTime,
                PatientName = appointment.Patient.User.Name,
                DoctorName = appointment.Doctor.User.Name
            };

            return Ok(appointmentsDto);
        }

        /// <summary>
        /// Crea una nueva cita médica.
        /// </summary>
        /// <remarks>
        /// Solo usuarios autorizados pueden crear citas.
        /// </remarks>
        /// <response code="200">Cita creada correctamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="500">Error interno</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("=== Usuario {UserId} intentando crear una cita ===", userId);

            var sanitizer = new HtmlSanitizer();

            if (!string.IsNullOrEmpty(createAppointmentDto.Reason))
            {
                createAppointmentDto.Reason = sanitizer.Sanitize(createAppointmentDto.Reason);
            }

            var doctorExists = await _doctorRepository.GetById(createAppointmentDto.DoctorId);

            if (doctorExists == null)
            {
                 _logger.LogWarning("=== Doctor {DoctorId} no existe ===", createAppointmentDto.DoctorId);
                return BadRequest(new { message = $"El id del doctor especificado {createAppointmentDto.DoctorId} no existe"});
            }

            var patientExists = await _patientRepository.GetById(createAppointmentDto.PatientId);

            if (patientExists == null)
            {
                _logger.LogWarning("=== Paciente {PatientId} no existe ===", createAppointmentDto.PatientId);
                return BadRequest(new { message = $"El id del paciente especificado {createAppointmentDto.PatientId} no existe" });
            }

            if (createAppointmentDto.AppointmentDateTime < DateTime.UtcNow)
            {
                _logger.LogWarning("=== Intento de crear cita en fecha pasada por usuario {UserId} ===", userId);
                return BadRequest("Debe ingresar una fecha y hora futura para la cita");
            }

            var appointment = createAppointmentDto.Adapt<Appointment>();

            appointment.Status = AppointmentStatus.Programada;

            var result = await _appointmentRepository.Add(appointment);

            if (!result)
            {
                _logger.LogError("=== Error al crear cita por usuario {UserId} ===", userId);
                return StatusCode(500, "Error al crear la cita");
            }

            _logger.LogInformation("=== Cita creada exitosamente con ID {AppointmentId} ===", appointment.AppointmentId);

            var appointmentDto = appointment.Adapt<AppointmentDto>();

            return Ok(appointmentDto);
        }

        
        [HttpPatch("{appointmentId:int}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            _logger.LogInformation("=== Actualizando estado de cita {AppointmentId} ===", appointmentId);
            
            var appointment = await _appointmentRepository.GetById(appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("=== Cita con id {AppointmentId} no encontrada para actualizar estado ===", appointmentId);
                return NotFound();
            }

            appointment.Status = updateAppointmentDto.Status;

            var result = await _appointmentRepository.Update(appointment);

            if (!result)
            {
                _logger.LogError("=== Error al actualizar estado de cita {AppointmentId} ===", appointmentId);
                return StatusCode(500, "Error al actualizar estado");
            }
        
             _logger.LogInformation("=== Estado de cita {AppointmentId} actualizado exitosamente a {Status} ===", appointmentId, updateAppointmentDto.Status);
            return NoContent();
        }

        [HttpGet("doctor/{doctorId:int}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("=== Consultando citas del doctor {DoctorId} entre {StartDate} y {EndDate} ===", doctorId, startDate, endDate);

            var appointments = await _appointmentRepository.GetByDoctorAndDate(doctorId, startDate, endDate);

            var appointmentsDto = appointments.Adapt<List<AppointmentDto>>();

            return Ok(appointmentsDto);
        }    

    }
}

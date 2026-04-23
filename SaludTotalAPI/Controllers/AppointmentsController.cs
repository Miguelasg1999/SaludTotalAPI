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

        public AppointmentsController(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentWithDetails(id);

            if (appointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            if (!User.IsInRole("Admin"))
            {
                
                if (User.IsInRole("Patient") && appointment.Patient.UserId != userId)
                {
                    return Forbid();
                }

                if (User.IsInRole("Doctor") && appointment.Doctor.UserId != userId)
                {
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

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {

            var sanitizer = new HtmlSanitizer();

            if (!string.IsNullOrEmpty(createAppointmentDto.Reason))
            {
                createAppointmentDto.Reason = sanitizer.Sanitize(createAppointmentDto.Reason);
            }

            var doctorExists = await _doctorRepository.GetById(createAppointmentDto.DoctorId);

            if (doctorExists == null)
            {
                return BadRequest(new { message = $"El id del doctor especificado {createAppointmentDto.DoctorId} no existe"});
            }

            var patientExists = await _patientRepository.GetById(createAppointmentDto.PatientId);

            if (patientExists == null)
            {
                return BadRequest(new { message = $"El id del paciente especificado {createAppointmentDto.PatientId} no existe" });
            }

            if (createAppointmentDto.AppointmentDateTime < DateTime.UtcNow)
            {
                return BadRequest("Debe ingresar una fecha y hora futura para la cita");
            }


            if (createAppointmentDto.AppointmentDateTime < DateTime.UtcNow)
            {
                return BadRequest("Debe ingresar una fecha y hora futura para la cita");
            }

            var appointment = createAppointmentDto.Adapt<Appointment>();

            appointment.Status = AppointmentStatus.Programada;

            var result = await _appointmentRepository.Add(appointment);

            if (!result)
            {
                return StatusCode(500, "Error al crear la cita");
            }

            var appointmentDto = appointment.Adapt<AppointmentDto>();

            return Ok(appointmentDto);
        }

        
        [HttpPatch("{appointmentId:int}/status")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateStatus(int appointmentId, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            var appointment = await _appointmentRepository.GetById(appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = updateAppointmentDto.Status;

            var result = await _appointmentRepository.Update(appointment);

            if (!result)
            {
                return StatusCode(500, "Error al actualizar estado");
            }

            return NoContent();
        }

        [HttpGet("doctor/{doctorId:int}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var appointments = await _appointmentRepository.GetByDoctorAndDate(doctorId, startDate, endDate);

            var appointmentsDto = appointments.Adapt<List<AppointmentDto>>();

            return Ok(appointmentsDto);
        }    

    }
}

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Roles = "Admin")]
    public class AppointmentsController : ControllerBase
    { 

        private readonly IAppointmentRepository _appointmentRepository;
        public AppointmentsController(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto createAppointmentDto)
        {
            if (!ModelState.IsValid)
            {   
                return BadRequest(ModelState);
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

        [Authorize(Roles = "Doctor")]
        [HttpPatch("{appointmentId:int}/status")]
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
        public async Task<IActionResult> GetByDoctor(int doctorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var citas = await _appointmentRepository.GetByDoctorAndDate(doctorId, startDate, endDate);

            var citasDto = citas.Adapt<List<AppointmentDto>>();

            return Ok(citasDto);
        }    

    }
}

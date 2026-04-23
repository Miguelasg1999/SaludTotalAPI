using System.Security.Claims;
using Asp.Versioning;
using Ganss.Xss;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPatientRepository _patientRepository;
        public MedicalRecordsController(IMedicalRecordRepository medicalRecordRepository, IPatientRepository patientRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _patientRepository = patientRepository;
        }


        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyMedicalRecord()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var record = await _medicalRecordRepository.GetByUserId(userId);

            if (record == null)
            {
                return NotFound();
            }

            var medicalRecordDto = new MedicalRecordDto
            {
                MedicalRecordId = record.MedicalRecordId,
                CreationDate = record.CreationDate,
                MedicalNotes = record.MedicalNotes,
                Allergies = record.Allergies,
                CurrentMedications = record.CurrentMedications
            };

            return Ok(medicalRecordDto);
        }

        [HttpGet("patient/{patientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin, Doctor, Patient")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var record = await _medicalRecordRepository.GetByPatientId(patientId);

            if (record == null)
            {
                return NotFound("El paciente no tiene expediente");
            }

            var medicalRecordDto = record.Adapt<MedicalRecordDto>();

            return Ok(medicalRecordDto);
        }


        [HttpPut("patient/{patientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin, Doctor")]
        public async Task<IActionResult> Upsert(int patientId, [FromBody] UpsertMedicalRecordDto upsertMedicalRecordDto)
        {
            var sanitizer = new HtmlSanitizer();
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingPatient = await _patientRepository.GetById(patientId);

            if(existingPatient == null)
            {
                    return NotFound("El paciente no existe");
            }

            var existingRecord = await _medicalRecordRepository.GetByPatientId(patientId);

            if (existingRecord == null)
            {
                upsertMedicalRecordDto.MedicalNotes = sanitizer.Sanitize(upsertMedicalRecordDto.MedicalNotes ?? "");
                upsertMedicalRecordDto.Allergies = sanitizer.Sanitize(upsertMedicalRecordDto.Allergies ?? "");
                upsertMedicalRecordDto.CurrentMedications = sanitizer.Sanitize(upsertMedicalRecordDto.CurrentMedications ?? "");

                var newRecord = upsertMedicalRecordDto.Adapt<MedicalRecord>();

                newRecord.PatientId = patientId;
                newRecord.CreationDate = DateTime.Now;

                var result = await _medicalRecordRepository.Add(newRecord);

                if (!result)
                {
                    ModelState.AddModelError("CustomError", "Error al crear expediente");
                    return StatusCode(500, ModelState);
                }

                var medicalRecordDto = newRecord.Adapt<MedicalRecordDto>();

                return Ok(medicalRecordDto);
            }

            upsertMedicalRecordDto.MedicalNotes = sanitizer.Sanitize(upsertMedicalRecordDto.MedicalNotes ?? "");
            upsertMedicalRecordDto.Allergies = sanitizer.Sanitize(upsertMedicalRecordDto.Allergies ?? "");
            upsertMedicalRecordDto.CurrentMedications = sanitizer.Sanitize(upsertMedicalRecordDto.CurrentMedications ?? "");

            upsertMedicalRecordDto.Adapt(existingRecord);

            var updateResult = await _medicalRecordRepository.Update(existingRecord);

            if (!updateResult)
            {
                ModelState.AddModelError("CustomError", "Error al actualizar expediente");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

    }
}

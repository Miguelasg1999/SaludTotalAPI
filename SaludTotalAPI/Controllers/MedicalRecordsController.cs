using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        public MedicalRecordsController(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }


        [HttpGet("patient/{patientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var record = await _medicalRecordRepository.GetByPatientId(patientId);

            if (record == null)
            {
                return NotFound("El paciente no tiene expediente");
            }

            var recordDto = record.Adapt<MedicalRecordDto>();

            return Ok(recordDto);
        }


        [HttpPut("patient/{patientId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Upsert(int patientId, [FromBody] UpsertMedicalRecordDto upsertMedicalRecordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingRecord = await _medicalRecordRepository.GetByPatientId(patientId);

            if (existingRecord == null)
            {
                var newRecord = upsertMedicalRecordDto.Adapt<MedicalRecord>();

                newRecord.PatientId = patientId;
                newRecord.CreationDate = DateTime.Now;

                var result = await _medicalRecordRepository.Add(newRecord);

                if (!result)
                {
                    ModelState.AddModelError("CustomError", "Error al crear expediente");
                    return StatusCode(500, ModelState);
                }

                var recordDto = newRecord.Adapt<MedicalRecordDto>();

                return Ok(recordDto);
            }

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

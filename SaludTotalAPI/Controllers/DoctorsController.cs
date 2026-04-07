using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        public DoctorsController(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateDoctor([FromForm] CreateDoctorDto createDoctorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctor = createDoctorDto.Adapt<Doctor>();

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (createDoctorDto.Photo != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createDoctorDto.Photo.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await createDoctorDto.Photo.CopyToAsync(stream);
                }

                doctor.PhotoUrl = fileName;
            }

            if (await _doctorRepository.EmailExists(createDoctorDto.Email))
            {
                ModelState.AddModelError("CustomError", "El email ya está registrado");
                return BadRequest(ModelState);
            }

            var result = await _doctorRepository.Add(doctor);

            if (!result)
            {
                return StatusCode(500, "Error al crear el médico");
            }

            var doctorDto = doctor.Adapt<DoctorDto>();

            return Ok(doctorDto);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctors(
            [FromQuery] int? specialtyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Los parámetros de paginación deben ser mayores a 0");
            }

            var doctors = await _doctorRepository.GetFiltered(specialtyId, page, pageSize);

            var doctorsDto = doctors.Adapt<List<DoctorDto>>();

            return Ok(doctorsDto);
        }

     }
}

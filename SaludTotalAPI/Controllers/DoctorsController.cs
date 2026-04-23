using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public DoctorsController(IDoctorRepository doctorRepository, UserManager<ApplicationUser> userManager)
        {
            _doctorRepository = doctorRepository;
            _userManager = userManager;
        }

        [HttpGet("{id}", Name = "GetDoctorById")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorRepository.GetDoctorById(id);

            if (doctor == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && doctor.UserId != userId)
            {
                return Forbid();
            }

            var doctorDto = new DoctorDto
            {
                DoctorId = doctor.DoctorId,
                Name = doctor.User.Name,
                Email = doctor.User.Email ?? "",
                Phone = doctor.Phone,
                Rut = doctor.User.Rut,
                PhotoUrl = doctor.PhotoUrl,
                SpecialtyId = doctor.SpecialtyId
            };

            return Ok(doctorDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctor([FromForm] CreateDoctorDto createDoctorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = createDoctorDto.Email,
                Email = createDoctorDto.Email,
                Name = createDoctorDto.Name,
                Rut = createDoctorDto.Rut
            };

            var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 10) + "Aa!";

            var resultUser = await _userManager.CreateAsync(user, tempPassword);

            if (!resultUser.Succeeded)
            {
                return BadRequest(new { message = "Error al crear el usuario", errors = resultUser.Errors.Select(e => e.Description) });
            }

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                UserId = user.Id,
                Phone = createDoctorDto.Phone,
                SpecialtyId = createDoctorDto.SpecialtyId
            };

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


            var resultDoctor = await _doctorRepository.Add(doctor);

            if (!resultDoctor)
            {
                return StatusCode(500, new { message = "Error al crear el médico" });
            }

            var doctorDto = new DoctorDto
            {
                DoctorId = doctor.DoctorId,
                Name = user.Name,
                Email = user.Email,
                Rut = user.Rut,
                Phone = doctor.Phone,
                PhotoUrl = doctor.PhotoUrl,
                SpecialtyId = doctor.SpecialtyId
            };

            var response = new CreateDoctorResponseDto
            {
                Doctor = doctorDto,
                Password = tempPassword
            };

            return CreatedAtRoute("GetDoctorById", new { id = doctor.DoctorId }, response);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin,Patient,Doctor")]
        public async Task<IActionResult> GetPagedDoctors([FromQuery] int? specialtyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Los parámetros de paginación deben ser mayores a 0");
            }

            var doctors = await _doctorRepository.GetPagedDoctors(specialtyId, page, pageSize);

            var doctorsDto = doctors.Select(d => new DoctorDto
            {
                DoctorId = d.DoctorId,
                Name = d.User.Name,
                Email = d.User.Email ?? string.Empty,
                Phone = d.Phone,
                Rut = d.User.Rut,
                PhotoUrl = d.PhotoUrl,
                SpecialtyId = d.SpecialtyId
            }).ToList();

            return Ok( new
            {
                Success = true,
                Message = "Médicos obtenidos exitosamente",
                Data = doctorsDto
            });
        }

    }
}

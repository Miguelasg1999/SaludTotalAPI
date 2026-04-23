using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;

        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(IPatientRepository patientRepository, UserManager<ApplicationUser> userManager)
        {
            _patientRepository = patientRepository;
            _userManager = userManager;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var patient = await _patientRepository.GetCurrentUser(userId);

             if (patient == null)
             {
                return NotFound();
            }

            var role = await _userManager.GetRolesAsync(patient.User);

            if (!role.Contains("Patient"))
            {
                return Forbid();
            }
            
            var patientDto = new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.User.Name,
                Rut = patient.User.Rut,
                Email = patient.User.Email ?? string.Empty,
                Phone = patient.Phone,
                Birthdate = patient.Birthdate
            };

            return Ok(patientDto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _patientRepository.GetPatients();

            var patientsDto = patients.Select(p => new PatientDto
            {
                PatientId = p.PatientId,
                Name = p.User.Name,
                Rut = p.User.Rut,
                Email = p.User.Email ?? string.Empty,
                Phone = p.Phone,
                Birthdate = p.Birthdate
            }).ToList();

            return Ok(patientsDto);
        }

        [HttpGet("{id}", Name = "GetPatientById")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _patientRepository.GetPatientById(id);

            if (patient == null)
            {
                return NotFound(); 
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isDoctor = User.IsInRole("Doctor");

            if (!isAdmin && !isDoctor && patient.UserId != userId)
            {
                return Forbid();
            }

            var patientDto = new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.User.Name,
                Email = patient.User.Email ?? string.Empty,
                Phone = patient.Phone,
                Birthdate = patient.Birthdate,
                Rut = patient.User.Rut
            };

            return Ok(patientDto);
        }

        [HttpGet("ByRut/{rut}", Name = "GetPatientByRut")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetPatientByRut(string rut)
        {
            var patient = await _patientRepository.GetPatientByRut(rut);

            if (patient == null)
            {
                return NotFound();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            var isDoctor = User.IsInRole("Doctor");

            if (!isAdmin && !isDoctor && patient.UserId != userId)
            {
                return Forbid();
            }

            var patientDto = new PatientDto
            {
                PatientId = patient.PatientId,
                Name = patient.User.Name,
                Email = patient.User.Email ?? string.Empty,
                Phone = patient.Phone,
                Birthdate = patient.Birthdate,
                Rut = patient.User.Rut
            };

            return Ok(patientDto);
        }
    }
}

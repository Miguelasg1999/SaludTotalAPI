using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPatientRepository _patientRepository;

        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IPatientRepository patientRepository, IConfiguration config)
        {
            _userManager = userManager;
            _patientRepository = patientRepository;
            _config = config;
        }

        private string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _config.GetValue<string>("Jwt:Key");
            var issuer = _config.GetValue<string>("Jwt:Issuer");
            var audience = _config.GetValue<string>("Jwt:Audience");
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new Exception("Firma JWT no configurada");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(7),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("fixed")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Username);

            if (user == null)
            {
                return Unauthorized($"El usuario con email {loginDto.Username} no existe");
            }
            
            var validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!validPassword)
            {
                return Unauthorized("Password incorrecta");
            }


            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateToken(user, roles);

            return Ok(new { token });
        }   

        [HttpPost("registerPatient")]
        [AllowAnonymous]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> RegisterPatient(RegisterPatientDto registerPatientDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                

            var existingUser = await _userManager.FindByEmailAsync(registerPatientDto.Email);

            if (existingUser != null)
            {
                return BadRequest($"El email {registerPatientDto.Email} ya está registrado");
            }
            var rutExists = await _userManager.Users.AnyAsync(u => u.Rut == registerPatientDto.Rut);

            if (rutExists)
            {
                return BadRequest($"El RUT {registerPatientDto.Rut} ya está registrado");
            }
                
            var user = new ApplicationUser
            {
                UserName = registerPatientDto.Email,
                Email = registerPatientDto.Email,
                Name = registerPatientDto.Name,
                Rut = registerPatientDto.Rut
            };

            var result = await _userManager.CreateAsync(user, registerPatientDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Patient");

            var patient = new Patient
            {
                UserId = user.Id,
                Phone = registerPatientDto.Phone,
                Birthdate = registerPatientDto.Birthdate
            };

            var resultPatient = await _patientRepository.Add(patient);

            if (!resultPatient)
            {
                return StatusCode(500, "Error al crear el paciente");
            }
                

            return Ok(new
            {
                message = $"{registerPatientDto.Name} registrado exitosamente"
            });
        }

        [HttpPost("changePassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);

            if (user == null)
            {
                return BadRequest($"El usuario con email {changePasswordDto.Email} no existe");
            }

            if(changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
            {
                return BadRequest("La nueva contraseña no puede ser igual a la actual");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Error al cambiar la contraseña. Verifique la contraseña actual e intente nuevamente.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            user.ChangePassword = false;
            await _userManager.UpdateAsync(user);

            return Ok( new
            {
                message = $"Contraseña de {user.Name} cambiada exitosamente"
            });
        }
    }
}

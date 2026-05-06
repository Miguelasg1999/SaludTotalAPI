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

        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IPatientRepository patientRepository, IConfiguration config, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _patientRepository = patientRepository;
            _config = config;
            _logger = logger;
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

        /// <summary>
        /// Autentica un usuario y retorna un token JWT.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/v1/auth/login
        ///     {
        ///        "username": "admin@saludtotal.com",
        ///        "password": "Admin123!"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Retorna el token JWT</response>
        /// <response code="401">Credenciales inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("fixed")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Username);

            _logger.LogInformation("=== Intento de login con email {Email} ===", loginDto.Username);

            if (user == null)
            {
                _logger.LogWarning("=== Login fallido: usuario {Email} no existe ===", loginDto.Username);
                return Unauthorized($"El usuario con email {loginDto.Username} no existe");
            }
            
            var validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!validPassword)
            {
                _logger.LogWarning("=== Login fallido: contraseña incorrecta para usuario {Email} ===", loginDto.Username);
                return Unauthorized("Password incorrecta");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateToken(user, roles);

            _logger.LogInformation("=== Login exitoso para usuario {UserId} ===", user.Id);

            return Ok(new
            {
                token,
                mustChangePassword = user.ChangePassword
            });
        }   

        [HttpPost("registerPatient")]
        [AllowAnonymous]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> RegisterPatient(RegisterPatientDto registerPatientDto)
        {
            _logger.LogInformation("=== Intento de registro para {Email} ===", registerPatientDto.Email);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }                

            var existingUser = await _userManager.FindByEmailAsync(registerPatientDto.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("=== Registro fallido: email {Email} ya está registrado ===", registerPatientDto.Email);

                return BadRequest($"El email {registerPatientDto.Email} ya está registrado");
            }
            var rutExists = await _userManager.Users.AnyAsync(u => u.Rut == registerPatientDto.Rut);

            if (rutExists)
            {
                _logger.LogWarning("=== Registro fallido: RUT ya está registrado {Rut} ===", registerPatientDto.Rut);
                
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

            _logger.LogInformation("=== Usuario {UserId} registrado correctamente ===", user.Id);

            return Ok(new
            {
                message = $"{registerPatientDto.Name} registrado exitosamente"
            });
        }

        [HttpPost("changePassword")]
        [AllowAnonymous]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            _logger.LogInformation("=== Intento de cambio de contraseña para {Email} ===", changePasswordDto.Email);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);

            if (user == null)
            {
                _logger.LogWarning("=== Error al cambiar la contraseña para {Email} no se encontro el usuario ===", changePasswordDto.Email);

                return BadRequest($"El usuario con email {changePasswordDto.Email} no existe");
            }

            if(changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
            {
                _logger.LogWarning("=== Error al cambiar la contraseña para {Email} la nueva contraseña es igual a la actual ===", changePasswordDto.Email);

                return BadRequest("La nueva contraseña no puede ser igual a la actual");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword
            );

            if (!result.Succeeded)
            {
                _logger.LogWarning("=== Error al cambiar la contraseña para {Email} ===", changePasswordDto.Email);

                return BadRequest(new
                {
                    message = "Error al cambiar la contraseña. Verifique la contraseña actual e intente nuevamente.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            user.ChangePassword = false;

            await _userManager.UpdateAsync(user);

            _logger.LogInformation("=== Contraseña de {Email} cambiada exitosamente ===", changePasswordDto.Email);

            return Ok( new
            {
                message = $"Contraseña de {user.Name} cambiada exitosamente"
            });
        }
    }
}

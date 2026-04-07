using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;

namespace SaludTotalAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController] 
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IConfiguration _config;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        //Metodo para generar token JWT
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Usuario no existe");
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

        [HttpPost("createRole")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _roleManager.RoleExistsAsync(roleDto.RoleName);

            if (exists)
                return BadRequest("El rol ya existe");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleDto.RoleName));

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol '{roleDto.RoleName}' creado");
        }

        [HttpPost("assignRole")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            var user = await _userManager.FindByNameAsync(assignRoleDto.Username);

            if (user == null)
            {
                return NotFound("Usuario no existe");
            }
        
            var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.RoleName);

            if (!roleExists)
            {
                return BadRequest("Rol no existe");
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(assignRoleDto.RoleName))
            {
                return BadRequest($"El rol {assignRoleDto.RoleName} ya está asignado");
            }
            
            var result = await _userManager.AddToRoleAsync(user, assignRoleDto.RoleName);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol '{assignRoleDto.RoleName}' asignado a '{assignRoleDto.Username}'");
        }


        [HttpGet("userRoles/{username}")]
        [MapToApiVersion("2.0")]
        public async Task<IActionResult> GetUserRoles(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(roles);
        }

    }
}

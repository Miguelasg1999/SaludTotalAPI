using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;

namespace SaludTotalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        private string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtKey = _config.GetValue<string>("Jwt:Key");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null)
            {
                return Unauthorized("Usuario no existe");
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!validPassword)
                return Unauthorized("Password incorrecta");

            var roles = await _userManager.GetRolesAsync(user);

            var token = GenerateToken(user, roles);

            return Ok(new { token });
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return BadRequest("Nombre de rol requerido");

            var exists = await _roleManager.RoleExistsAsync(roleName);

            if (exists)
                return BadRequest("El rol ya existe");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol '{roleName}' creado");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return NotFound("Usuario no existe");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
                return BadRequest("Rol no existe");

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok($"Rol '{roleName}' asignado a '{username}'");
        }

        [HttpGet("user-roles/{username}")]
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

using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;

namespace SaludTotalAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [ApiVersion("2.0")]
    public class RoleController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("{id}", Name = "GetRoleById")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var roleResponseDto = role.Adapt<RoleResponseDto>();

            return Ok(roleResponseDto);
        }

        [HttpPost("createRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exists = await _roleManager.RoleExistsAsync(roleDto.RoleName);

            if (exists)
            {
                return BadRequest($"El rol '{roleDto.RoleName}' ya existe");
            }
            
            var role = new IdentityRole(roleDto.RoleName);

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
                
            }

            var roleResponseDto = role.Adapt<RoleResponseDto>();

            return CreatedAtRoute("GetRoleById", new { id = roleResponseDto.Id }, roleResponseDto);
        }

        [HttpPost("assignRole")]
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
        public async Task<IActionResult> GetUserRoles(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new 
            {
                username,
                roles
            });
        }
    }
}

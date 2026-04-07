using Asp.Versioning;
using Ganss.Xss;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaludTotalAPI.Constants;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Authorize]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyRepository _specialtyRepository;
        public SpecialtiesController(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = CacheProfiles.Default60)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var specialties = await _specialtyRepository.GetAll();
            var specialtiesDto = specialties.Adapt<IEnumerable<SpecialtyDto>>();
            return Ok(specialtiesDto);
        }

        [HttpGet("{specialtyId:int}", Name = "GetById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int specialtyId)
        {
            var specialty = await _specialtyRepository.GetById(specialtyId);
            
            if(specialty == null)
            {
                return NotFound($"La especialidad con el id {specialtyId} no existe");
            }

            var specialtyDto = specialty.Adapt<SpecialtyDto>();
            return Ok(specialtyDto);
        }

        [HttpGet("doctors/{specialtyId:int}", Name = "GetWithDoctors")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AllowAnonymous]
        public async Task<IActionResult> GetWithDoctors(int specialtyId)
        {
            var specialty = await _specialtyRepository.GetWithDoctors(specialtyId);
            
            if(specialty == null)
            {
                return NotFound($"La especialidad con el id {specialtyId} no existe");
            }

            var specialtyWithDoctors = specialty.Adapt<SpecialtyWithDoctorsDto>();
            return Ok(specialtyWithDoctors);
        }

        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDto createSpecialtyDto)
        {
            var sanitizer = new HtmlSanitizer();
                
            if (createSpecialtyDto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _specialtyRepository.SpecialtyExists(createSpecialtyDto.Name))
            {
                ModelState.AddModelError("CustomError", "La especialidad ya existe");
                return BadRequest(ModelState);
            }

            createSpecialtyDto.Description = sanitizer.Sanitize(createSpecialtyDto.Description ?? "");

            var specialty = createSpecialtyDto.Adapt<Specialty>();
            
            var result = await _specialtyRepository.Add(specialty);

            if(!result)
            {
                ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el registro {specialty.Name}");
                return StatusCode(500, ModelState);
            }

            var specialtyDto = specialty.Adapt<SpecialtyDto>();
            return CreatedAtRoute("GetById", new { specialtyId = specialty.SpecialtyId }, specialtyDto);
        }

        [HttpPut("{specialtyId:int}", Name = "UpdateSpecialty")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSpecialty(int specialtyId, [FromBody] UpdateSpecialtyDto updateSpecialtyDto)
{
        var sanitizer = new HtmlSanitizer();
        
        if (updateSpecialtyDto == null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specialty = await _specialtyRepository.GetById(specialtyId);

        if (specialty == null)
        {
            return NotFound();
        }

        if (await _specialtyRepository.SpecialtyExists(updateSpecialtyDto.Name) 
            && specialty.Name != updateSpecialtyDto.Name)
        {
            ModelState.AddModelError("CustomError", "La especialidad ya existe");
            return BadRequest(ModelState);
        }

        updateSpecialtyDto.Description = sanitizer.Sanitize(updateSpecialtyDto.Description ?? "");
        updateSpecialtyDto.Adapt(specialty);

        var result = await _specialtyRepository.Update(specialty);

        if (!result)
        {
            ModelState.AddModelError("CustomError", "Error al actualizar");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }

    [HttpDelete("{specialtyId:int}", Name = "DeleteSpecialty")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSpecialty(int specialtyId)
    {
      if (specialtyId == 0)
      {
        return BadRequest(ModelState);
      }

      var specialty = await _specialtyRepository.GetById(specialtyId);
      if (specialty == null)
      {
        return NotFound($"La especialidad con el id {specialtyId} no existe");
      }

      if(await _specialtyRepository.HasDoctor(specialtyId))
      {
        ModelState.AddModelError("CustomError", $"No se puede eliminar la especialidad {specialty.Name} porque tiene medicos asociados");
        return BadRequest(ModelState);
      }

      if (!await _specialtyRepository.Delete(specialty))
      {
        ModelState.AddModelError("CustomError", $"Algo salió mal al eliminar la especialidad {specialty.Name}");
        return StatusCode(500, ModelState);
      }
      return NoContent();
    }



    } //end SpecialtiesController
}

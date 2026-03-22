using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaludTotalAPI.Models;
using SaludTotalAPI.Models.Dtos;
using SaludTotalAPI.Repository;
using SaludTotalAPI.Repository.IRepository;

namespace SaludTotalAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyRepository _specialtyRepository;
        public SpecialtiesController(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDto createSpecialtyDto)
        {
        if (createSpecialtyDto == null)
        {
            return BadRequest(ModelState);
        }



        if (_specialtyRepository.SpecialtyExists(createSpecialtyDto.Name))
        {
            ModelState.AddModelError("CustomError", "La especialidad ya existe");
            return BadRequest(ModelState);
        }

        var specialty = createSpecialtyDto.Adapt<Specialty>();
        
        var result = _specialtyRepository.Add(specialty);

        if(!await result)
        {
            ModelState.AddModelError("CustomError", $"Algo salió mal al guardar el registro {specialty.Name}");
            return StatusCode(500, ModelState);
        }
        return CreatedAtRoute("GetById", new { id = specialty.SpecialtyId }, specialty);
        }

        /*[HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromForm] UpdateProductDto updateProductDto)
        {
        if (updateProductDto == null)
        {
            return BadRequest(ModelState);
        }
        if (!_productRepository.ProductExists(productId))
        {
            ModelState.AddModelError("CustomError", "El producto no existe");
            return BadRequest(ModelState);
        }
        if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
        {
            ModelState.AddModelError("CustomError", $"La categoría con el {updateProductDto.CategoryId} no existe");
            return BadRequest(ModelState);
        }
        var product = updateProductDto.Adapt<Product>();
        product.ProductId = productId;
        // Agregando imagen
        if (updateProductDto.Image != null)
        {
            UploadProductImage(updateProductDto, product);
        }
        else
        {
            product.ImgUrl = "https://placehold.co/300x300";
        }
        if (!_productRepository.UpdateProduct(product))
        {
            ModelState.AddModelError("CustomError", $"Algo salió mal al actualizar el registro {product.Name}");
            return StatusCode(500, ModelState);
        }
        return NoContent();
        }*/


    } //end SpecialtiesController
}

using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class CreateDoctorDto
{
    [Required(ErrorMessage = "El nombre completo del doctor es obligatorio")]
    [StringLength(100, MinimumLength = 15, ErrorMessage = "El nombre completo debe tener entre 20 y 100 caracteres")]
    public string FullName { get; set; } = string.Empty;
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public IFormFile? Photo { get; set; }
    public int SpecialtyId { get; set; }
}

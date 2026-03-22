using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class UpdateSpecialtyDto
{
    [Required(ErrorMessage = "El nombre de especialidad es obligatorio")]
    [MaxLength(30, ErrorMessage = "El nombre de especialidad no puede tener más de 30 caracteres")]
    [MinLength(5, ErrorMessage = "El nombre de especialidad no puede tener menos de 5 caracteres")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

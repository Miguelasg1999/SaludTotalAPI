using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class ChangePasswordDto
{
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingrese la contraseña actual, es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Ingrese la nueva contraseña, es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;
}

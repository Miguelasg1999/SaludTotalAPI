using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class RegisterPatientDto
{
    [Required(ErrorMessage = "El nombre completo del doctor es obligatorio")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "El nombre completo debe tener entre 10 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    [Required]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido")]
    public string Email { get; set; } = string.Empty;
    [Required(ErrorMessage = "El RUT del doctor es obligatorio")]
    [MaxLength(12, ErrorMessage = "El RUT no puede tener más de 12 caracteres")]
    public string Rut { get; set; } = string.Empty;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]

    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public DateTime Birthdate { get; set; }
}

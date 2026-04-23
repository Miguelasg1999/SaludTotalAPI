using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class LoginDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [EmailAddress(ErrorMessage = "El nombre de usuario debe ser una dirección de correo electrónico válida.")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

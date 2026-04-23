using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SaludTotalAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(12, ErrorMessage = "El RUT no puede tener más de 12 caracteres.")]
    public string Rut { get; set; } = string.Empty;
    public bool ChangePassword { get; set; } = true;

    public Doctor? Doctor { get; set; }
    public Patient? Patient { get; set; }
}

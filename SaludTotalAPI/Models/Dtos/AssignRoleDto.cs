using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class AssignRoleDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "El nombre de rol es requerido")]
    public string RoleName { get; set; } = null!;
}

using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class RoleDto
{
    [Required]
    public string RoleName { get; set; } = null!;
}

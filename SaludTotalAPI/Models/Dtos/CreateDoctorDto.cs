using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models.Dtos;

public class CreateDoctorDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public IFormFile? Photo { get; set; }
    public int SpecialtyId { get; set; }
}

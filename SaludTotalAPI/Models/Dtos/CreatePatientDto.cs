using System;

namespace SaludTotalAPI.Models.Dtos;

public class CreatePatientDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public DateTime Birthdate { get; set; }
}

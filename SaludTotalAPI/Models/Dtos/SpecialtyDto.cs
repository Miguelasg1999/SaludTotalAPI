using System;

namespace SaludTotalAPI.Models.Dtos;

public class SpecialtyDto
{
    public int SpecialtyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

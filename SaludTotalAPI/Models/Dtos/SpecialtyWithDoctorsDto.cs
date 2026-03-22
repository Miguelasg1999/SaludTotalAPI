using System;

namespace SaludTotalAPI.Models.Dtos;

public class SpecialtyWithDoctorsDto
{
    public int SpecialtyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<DoctorDto> Doctors { get; set; } = new List<DoctorDto>();
}

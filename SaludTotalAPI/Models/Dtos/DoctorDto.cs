using System;

namespace SaludTotalAPI.Models.Dtos;

public class DoctorDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string? PhotoUrl { get; set; }
    public int SpecialtyId { get; set; }
}

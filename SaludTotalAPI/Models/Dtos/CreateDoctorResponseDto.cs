using System;

namespace SaludTotalAPI.Models.Dtos;

public class CreateDoctorResponseDto
{
    public required DoctorDto Doctor { get; set; }
    public required string Password { get; set; }
}

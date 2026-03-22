using System;

namespace SaludTotalAPI.Models.Dtos;

public class LoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

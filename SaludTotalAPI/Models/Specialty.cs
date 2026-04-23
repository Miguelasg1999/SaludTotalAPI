using System;
using System.ComponentModel.DataAnnotations;

namespace SaludTotalAPI.Models;

public class Specialty
{
    [Key]
    public int SpecialtyId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}

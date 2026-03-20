using System;
using Microsoft.AspNetCore.Identity;

namespace SaludTotalAPI.Models;

public class ApplicationUser: IdentityUser
{
    public string Name { get; set; } = string.Empty;
}

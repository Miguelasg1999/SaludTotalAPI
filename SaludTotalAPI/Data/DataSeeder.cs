using System;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        //await context.Database.EnsureDeletedAsync();
        //await context.Database.MigrateAsync();

        var specialties = new List<Specialty>
    {
        new Specialty { Name = "Cardiología", Description = "Especialidad del corazón" },
        new Specialty { Name = "Pediatría", Description = "Atención a niños" },
        new Specialty { Name = "Dermatología", Description = "Enfermedades de la piel" }
    };

    foreach (var specialty in specialties)
    {
        var exists = await context.Specialties
            .AnyAsync(s => s.Name == specialty.Name);

        if (!exists)
        {
            await context.Specialties.AddAsync(specialty);
        }
    }
            await context.SaveChangesAsync();
    }
}
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        //await db.Database.EnsureDeletedAsync();
        //await db.Database.MigrateAsync();

        var specialties = new List<Specialty>
        {
            new Specialty { Name = "Cardiología", Description = "Especialidad del corazón" },
            new Specialty { Name = "Pediatría", Description = "Atención a niños" },
            new Specialty { Name = "Dermatología", Description = "Enfermedades de la piel" }
        };

        foreach (var specialty in specialties)
        {
            var exists = await db.Specialties
                .AnyAsync(s => s.Name == specialty.Name);

            if (!exists)
            {
                await db.Specialties.AddAsync(specialty);
            }
        }

        string[] roles = { "Admin", "Doctor", "Patient" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminUser = await userManager.FindByNameAsync("admin");

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@saludtotal.com",
                Name = "Administrador"
            };

            var result = await userManager.CreateAsync(user, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        var doctorUser = await userManager.FindByNameAsync("doctor");

        if (doctorUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "doctor",
                Email = "doctor@saludtotal.com",
                Name = "Doctor Demo"
            };

            var result = await userManager.CreateAsync(user, "Doctor123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Doctor");
            }
        }

        await db.SaveChangesAsync();
    }
}
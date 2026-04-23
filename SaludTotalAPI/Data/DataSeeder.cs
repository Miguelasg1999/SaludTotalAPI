using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Enums;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        //SPECIALTIES
        if (!await db.Specialties.AnyAsync())
        {
            var specialties = new List<Specialty>
            {
                new Specialty { Name = "Cardiología", Description = "Especialidad del corazón" },
                new Specialty { Name = "Pediatría", Description = "Atención a niños" },
                new Specialty { Name = "Dermatología", Description = "Enfermedades de la piel" }
            };

            await db.Specialties.AddRangeAsync(specialties);
            await db.SaveChangesAsync();
        }

        //ROLES
        string[] roles = { "Admin", "Doctor", "Patient" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        //ADMIN
        var adminEmail = "admin@saludtotal.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Administrador",
                Rut = "11111111-1"
            };

            var password = GeneratePassword();

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");

                Console.WriteLine("ADMIN CREADO:");
                Console.WriteLine($"Email: {adminEmail}");
                Console.WriteLine($"Password: {password}");
            }
        }

        //DOCTOR
        var doctorEmail = "doctor@saludtotal.com";
        var doctorUser = await userManager.FindByEmailAsync(doctorEmail);

        if (doctorUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = doctorEmail,
                Email = doctorEmail,
                Name = "Doctor Demo",
                Rut = "22222222-2"
            };

            var password = GeneratePassword();

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Doctor");

                var specialty = await db.Specialties.FirstOrDefaultAsync();

                if(specialty != null)
                {
                    var doctor = new Doctor
                    {
                        UserId = user.Id,
                        Phone = "123456789",
                        SpecialtyId = specialty.SpecialtyId
                    };

                    await db.Doctors.AddAsync(doctor);
                    await db.SaveChangesAsync();
                }

                Console.WriteLine("DOCTOR CREADO:");
                Console.WriteLine($"Email: {doctorEmail}");
                Console.WriteLine($"Password: {password}");
            }
        }

        //PATIENT
        var patientEmail = "patient@saludtotal.com";
        var patientUser = await userManager.FindByEmailAsync(patientEmail);

        if (patientUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = patientEmail,
                Email = patientEmail,
                Name = "Paciente Demo",
                Rut = "33333333-3"
            };

            var password = GeneratePassword();

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    UserId = user.Id,
                    Phone = "987654321",
                    Birthdate = DateTime.Now.AddYears(-25)
                };

                await db.Patients.AddAsync(patient);
                await db.SaveChangesAsync();

                Console.WriteLine("PATIENT CREADO:");
                Console.WriteLine($"Email: {patientEmail}");
                Console.WriteLine($"Password: {password}");
            }
        }

        //MEDICALRECORD
        var patientUserDb = await userManager.FindByEmailAsync(patientEmail);

        if (patientUserDb != null)
        {
            var existingPatient = await db.Patients
                .FirstOrDefaultAsync(p => p.UserId == patientUserDb.Id);

            if (existingPatient != null)
            {
                var recordExists = await db.MedicalRecords
                    .AnyAsync(m => m.PatientId == existingPatient.PatientId);

                if (!recordExists)
                {
                    var medicalRecord = new MedicalRecord
                    {
                        PatientId = existingPatient.PatientId,
                        CreationDate = DateTime.UtcNow,
                        MedicalNotes = "Paciente sin antecedentes relevantes",
                        Allergies = "Ninguna",
                        CurrentMedications = "Ninguno"
                    };

                    await db.MedicalRecords.AddAsync(medicalRecord);
                    await db.SaveChangesAsync();
                }
            }
        }


        //APPOINTMENT
        var doctorEntity = await db.Doctors.FirstOrDefaultAsync();
        var patientEntity = await db.Patients.FirstOrDefaultAsync();

        if (doctorEntity != null && patientEntity != null)
        {
            var appointmentExists = await db.Appointments.AnyAsync();

            if (!appointmentExists)
            {
                var appointment = new Appointment
                {
                    DoctorId = doctorEntity.DoctorId,
                    PatientId = patientEntity.PatientId,
                    AppointmentDateTime = DateTime.UtcNow.AddDays(1),
                    Reason = "Chequeo general",
                    Status = AppointmentStatus.Programada
                };

                await db.Appointments.AddAsync(appointment);
                await db.SaveChangesAsync();
            }
        }

    }

    private static string GeneratePassword()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 8) + "Aa!";
    }
}
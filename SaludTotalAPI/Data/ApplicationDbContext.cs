using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaludTotalAPI.Models;

namespace SaludTotalAPI.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Rut)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.MedicalRecord)
            .WithOne(m => m.Patient)
            .HasForeignKey<MedicalRecord>(m => m.PatientId);

            modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<string>();

         modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId);

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId);

    }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
}
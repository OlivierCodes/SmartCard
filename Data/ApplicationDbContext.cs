using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCard.Models;

namespace SmartCard.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<FuelQuota> FuelQuotas { get; set; }
        public DbSet<Consumption> Consumptions { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Employee configuration
            builder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.MonthlyFuelQuota).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.EmployeeNumber).IsUnique();
                
                // Configure the Department relationship
                entity.HasOne(e => e.Department)
                      .WithMany(d => d.Employees)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Department configuration
            builder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
                entity.Property(d => d.CreatedDate).HasDefaultValueSql(Database.IsNpgsql() ? "CURRENT_TIMESTAMP" : "GETDATE()");
            });

            // Card configuration
            builder.Entity<Card>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.CardNumber).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Status).HasDefaultValue(CardStatus.Active);
                
                entity.HasIndex(c => c.CardNumber).IsUnique();
                
                entity.HasOne(c => c.Employee)
                      .WithMany(e => e.Cards)
                      .HasForeignKey(c => c.EmployeeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // FuelQuota configuration
            builder.Entity<FuelQuota>(entity =>
            {
                entity.HasKey(fq => fq.Id);
                entity.Property(fq => fq.Month).IsRequired();
                entity.Property(fq => fq.Year).IsRequired();
                entity.Property(fq => fq.QuotaInLiters).HasColumnType("decimal(10,2)");
                entity.Property(fq => fq.UsedLiters).HasColumnType("decimal(10,2)");
                
                entity.HasOne(fq => fq.Employee)
                      .WithMany(e => e.FuelQuotas)
                      .HasForeignKey(fq => fq.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Composite index to ensure one quota per employee per month/year
                entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year }).IsUnique();
            });

            // Consumption configuration
            builder.Entity<Consumption>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.AmountInLiters).HasColumnType("decimal(10,2)");
                entity.Property(c => c.TransactionDate).HasDefaultValueSql(Database.IsNpgsql() ? "CURRENT_TIMESTAMP" : "GETDATE()");
                entity.Property(c => c.Description).HasMaxLength(200);
                
                entity.HasOne(c => c.Employee)
                      .WithMany(e => e.Consumptions)
                      .HasForeignKey(c => c.EmployeeId)
                      .OnDelete(DeleteBehavior.NoAction);
                
                entity.HasOne(c => c.Card)
                      .WithMany(c => c.Consumptions)
                      .HasForeignKey(c => c.CardId)
                      .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
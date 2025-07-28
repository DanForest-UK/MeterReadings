using MeterReading.Domain;
using MeterReading.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReading.Infrastructure.Data
{
    public class MeterReadingContext : DbContext
    {
        public MeterReadingContext(DbContextOptions<MeterReadingContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Domain.MeterReading> MeterReadings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                // Configure AccountId value object
                entity.Property(e => e.AccountId)
                      .HasConversion(
                          accountId => accountId.Value,
                          value => new AccountId(value));

                // Configure Person value object
                entity.OwnsOne(e => e.Person, person =>
                {
                    person.Property(p => p.FirstName)
                        .HasColumnName("FirstName")
                        .IsRequired()
                        .HasMaxLength(100);

                    person.Property(p => p.LastName)
                        .HasColumnName("LastName")
                        .IsRequired()
                        .HasMaxLength(100);
                });
            });

            // MeterReading configuration
            modelBuilder.Entity<Domain.MeterReading>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Configure MeterReadingId value object - force database generation
                entity.Property(e => e.Id)
                      .HasConversion(
                          id => id.Value,
                          value => new MeterReadingId(value))
                      .ValueGeneratedOnAdd()
                      .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore); // This forces EF to ignore the value during insert

                // Configure AccountId value object
                entity.Property(e => e.AccountId)
                      .HasConversion(
                          accountId => accountId.Value,
                          value => new AccountId(value));

                // Configure MeterReadValue value object
                entity.Property(e => e.MeterReadValue)
                      .HasConversion(
                          meterValue => meterValue.Value,
                          value => new MeterReadValue(value));

                // Configure indexes and constraints
                entity.HasIndex(e => new { e.AccountId, e.MeterReadingDateTime }).IsUnique();

                // Foreign key relationship (no navigation properties)
                entity.HasOne<Account>()
                      .WithMany()
                      .HasForeignKey(e => e.AccountId)
                      .HasPrincipalKey(a => a.AccountId);
            });
        }
    }
}
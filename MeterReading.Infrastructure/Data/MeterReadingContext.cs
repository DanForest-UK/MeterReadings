using MeterReading.Domain;
using MeterReading.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeterReading.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework DbContext for the Meter Reading system.
    /// Manages the data access layer and entity configurations for accounts and meter readings.
    /// </summary>
    public class MeterReadingContext : DbContext
    {
        public const int MAX_NAME_LENGTH = 100;

        /// <summary>
        /// Initializes a new instance of the MeterReadingContext class.
        /// </summary>
        /// <param name="options">The options for this context</param>
        public MeterReadingContext(DbContextOptions<MeterReadingContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the DbSet for Account entities.
        /// Represents the collection of customer accounts in the system.
        /// </summary>
        public DbSet<Account> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for MeterReading entities.
        /// Represents the collection of meter readings associated with customer accounts.
        /// </summary>
        public DbSet<Domain.MeterReading> MeterReadings { get; set; }


        /// <summary>
        /// Configures the model and entity relationships for the database context.
        /// Sets up value object conversions, constraints, indexes, and foreign key relationships.
        /// </summary>        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);

                entity.Property(e => e.AccountId)
                      .HasConversion(
                          accountId => accountId.Value,
                          value => new AccountId(value));

                entity.OwnsOne(e => e.Person, person =>
                {
                    person.Property(p => p.FirstName)
                        .HasColumnName("FirstName")
                        .IsRequired()
                        .HasMaxLength(MAX_NAME_LENGTH);

                    person.Property(p => p.LastName)
                        .HasColumnName("LastName")
                        .IsRequired()
                        .HasMaxLength(MAX_NAME_LENGTH);
                });
            });

            // MeterReading configuration
            modelBuilder.Entity<Domain.MeterReading>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Forces EF to ignore the domain value and let the database generate the ID
                entity.Property(e => e.Id)
                      .HasConversion(
                          id => id.Value,
                          value => new MeterReadingId(value))
                      .ValueGeneratedOnAdd()
                      .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

                // Configure AccountId value object conversion for foreign key relationship
                entity.Property(e => e.AccountId)
                      .HasConversion(
                          accountId => accountId.Value,
                          value => new AccountId(value));

                entity.Property(e => e.MeterReadValue)
                      .HasConversion(
                          meterValue => meterValue.Value,
                          value => new MeterReadValue(value));

                // Create unique composite index to prevent duplicate readings for same account and datetime
                entity.HasIndex(e => new { e.AccountId, e.MeterReadingDateTime }).IsUnique();

                // Configure foreign key relationship to Account
                entity.HasOne<Account>()
                      .WithMany()
                      .HasForeignKey(e => e.AccountId)
                      .HasPrincipalKey(a => a.AccountId);
            });
        }
    }
}
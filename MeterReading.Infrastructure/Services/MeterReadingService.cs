using CsvHelper;
using LanguageExt;
using MeterReading.Domain;
using MeterReading.Infrastructure.Data;
using MeterReading.Infrastructure.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterReading.Infrastructure.Services
{

    /// <summary>
    /// Processes meter readings from a CSV stream with all-or-nothing validation
    /// </summary>
    public class MeterReadingService : IMeterReadingService
    {
        readonly MeterReadingContext context;

        public MeterReadingService(MeterReadingContext context) => this.context = context;

        /// <summary>
        /// Processes meter readings from a CSV stream with all-or-nothing validation and commit
        /// </summary>
        public async Task<ProcessingResult> ProcessMeterReadingsAsync(Stream csvStream)
        {
            var errors = new List<string>();
            var validatedReadings = new List<MeterReading.Domain.MeterReading>();
            var validAccountIds = await (from account in context.Accounts
                                         select account.AccountId).ToHashSetAsync();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<dynamic>();

            // First pass: validate all records without committing
            foreach (var record in records)
            {
                try
                {
                    var accountId = (string)record.AccountId;
                    var dateTimeString = (string)record.MeterReadingDateTime;
                    var meterValue = (string)record.MeterReadValue;

                    var accountValidation = MeterReadingValidator.ValidateAccountExists(accountId, validAccountIds);
                    if (!accountValidation.IsSuccess)
                    {
                        errors.Add(accountValidation.ErrorMessage);
                        continue;
                    }

                    var dateTimeValidation = MeterReadingValidator.ValidateDateTime(dateTimeString);
                    if (!dateTimeValidation.IsSuccess)
                    {
                        errors.Add($"Account {accountId}: {dateTimeValidation.ErrorMessage}");
                        continue;
                    }

                    var meterValueValidation = MeterReadingValidator.ValidateMeterReadValue(meterValue);
                    if (!meterValueValidation.IsSuccess)
                    {
                        errors.Add($"Account {accountId}: {meterValueValidation.ErrorMessage}");
                        continue;
                    }

                    var existingReading = await (from reading in context.MeterReadings
                                                 where reading.AccountId == accountValidation.Value && reading.MeterReadingDateTime == dateTimeValidation.Value
                                                 select reading).FirstOrDefaultAsync();

                    if (existingReading != null)
                    {
                        errors.Add($"Duplicate reading for account {accountId} at {dateTimeString}");
                        continue;
                    }

                    var meterReading = new MeterReading.Domain.MeterReading(
                        new MeterReadingId(0),
                        accountValidation.Value,
                        dateTimeValidation.Value,
                        meterValueValidation.Value);

                    validatedReadings.Add(meterReading);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing record: {ex.Message}");
                }
            }

            var failedCount = errors.Count;
            var validatedCount = validatedReadings.Count;

            // Only commit if there are no validation errors
            if (errors.Count == 0)
            {
                try
                {
                    using var transaction = await context.Database.BeginTransactionAsync();

                    context.MeterReadings.AddRange(validatedReadings);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return new ProcessingResult(validatedCount, failedCount, validatedCount, errors.ToArray());
                }
                catch (Exception ex)
                {
                    // Database save failed - nothing was committed due to transaction rollback
                    return new ProcessingResult(validatedCount, 1, 0, $"Database save failed: {ex.Message}".Cons().ToArray());
                }
            }

            // Validation failed - nothing committed
            return new ProcessingResult(validatedCount, failedCount, 0, errors.ToArray());
        }
    }
}


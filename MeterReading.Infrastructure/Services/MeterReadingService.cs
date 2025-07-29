using CsvHelper;
using LanguageExt;
using MeterReading.Domain;
using MeterReading.Infrastructure.Data;
using MeterReading.Infrastructure.Validation;
using MeterReading.Infrastructure.Validation.MeterReading.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MeterReading.Infrastructure.Services
{
    /// <summary>
    /// Processes meter readings from a CSV stream with all-or-nothing validation
    /// </summary>
    public class MeterReadingService : IMeterReadingService
    {
        readonly MeterReadingContext context;
        readonly Cache cache;

        /// <summary>
        /// Initializes a new instance of the MeterReadingService
        /// </summary>
        public MeterReadingService(MeterReadingContext context, Cache cache)
        {
            this.context = context;
            this.cache = cache;
        }

        /// <summary>
        /// Processes meter readings from a CSV stream with all-or-nothing validation and commit
        /// </summary>
        public async Task<ProcessingResult> ProcessMeterReadingsAsync(Stream csvStream)
        {
            var errors = new List<string>();
            var validatedReadings = new List<Domain.MeterReading>();
            var validAccountIds = cache.GetAccountIds();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // First pass: validate all records without committing
            foreach (var record in csv.GetRecords<dynamic>())
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
                                                 where reading.AccountId == accountValidation.Value &&
                                                       reading.MeterReadingDateTime == dateTimeValidation.Value
                                                 select reading).FirstOrDefaultAsync();

                    if (existingReading != null)
                    {
                        errors.Add($"Duplicate reading for account {accountId} at {dateTimeString}");
                        continue;
                    }

                    validatedReadings.Add(new Domain.MeterReading(
                        new MeterReadingId(0),
                        accountValidation.Value,
                        dateTimeValidation.Value,
                        meterValueValidation.Value));
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    errors.Add("Error processing record, please contact support");
                }
            }

            var failedCount = errors.Count;
            var validatedCount = validatedReadings.Count;

            return errors.Count == 0
                ? await CommitValidatedReadings(validatedReadings, validatedCount, failedCount)
                : new ProcessingResult(validatedCount, failedCount, 0, errors.ToArray());
        }

        /// <summary>
        /// Commits validated readings to the database within a transaction
        /// </summary>
        async Task<ProcessingResult> CommitValidatedReadings(List<Domain.MeterReading> validatedReadings, int validatedCount, int failedCount)
        {
            try
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                context.MeterReadings.AddRange(validatedReadings);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ProcessingResult(validatedCount, failedCount, validatedCount, Array.Empty<string>());
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new ProcessingResult(validatedCount, 1, 0, new[] { $"Database save failed, please contact support" });
            }
        }
    }
}
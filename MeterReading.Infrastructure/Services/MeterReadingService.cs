using CsvHelper;
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
    public class MeterReadingService : IMeterReadingService
    {
        readonly MeterReadingContext context;

        public MeterReadingService(MeterReadingContext context) => this.context = context;

        /// <summary>
        /// Processes meter readings from a CSV stream and returns processing results
        /// </summary>
        public async Task<ProcessingResult> ProcessMeterReadingsAsync(Stream csvStream)
        {
            var errors = new List<string>();
            var successfulReadings = 0;
            var failedReadings = 0;

            var validAccountIds = await (from account in context.Accounts
                                         select account.AccountId).ToHashSetAsync();

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<dynamic>();

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
                        failedReadings++;
                        errors.Add(accountValidation.ErrorMessage);
                        continue;
                    }

                    var dateTimeValidation = MeterReadingValidator.ValidateDateTime(dateTimeString);
                    if (!dateTimeValidation.IsSuccess)
                    {
                        failedReadings++;
                        errors.Add($"Account {accountId}: {dateTimeValidation.ErrorMessage}");
                        continue;
                    }

                    var meterValueValidation = MeterReadingValidator.ValidateMeterReadValue(meterValue);
                    if (!meterValueValidation.IsSuccess)
                    {
                        failedReadings++;
                        errors.Add($"Account {accountId}: {meterValueValidation.ErrorMessage}");
                        continue;
                    }

                    var existingReading = await (from reading in context.MeterReadings
                                                 where reading.AccountId == accountValidation.Value && reading.MeterReadingDateTime == dateTimeValidation.Value
                                                 select reading).FirstOrDefaultAsync();

                    if (existingReading != null)
                    {
                        failedReadings++;
                        errors.Add($"Duplicate reading for account {accountId} at {dateTimeString}");
                        continue;
                    }

                    var meterReading = new MeterReading.Domain.MeterReading(
                        new MeterReadingId(0),
                        accountValidation.Value,
                        dateTimeValidation.Value,
                        meterValueValidation.Value);

                    context.MeterReadings.Add(meterReading);
                    successfulReadings++;
                }
                catch (Exception ex)
                {
                    failedReadings++;
                    errors.Add($"Error processing record: {ex.Message}");
                }
            }

            await context.SaveChangesAsync();
            return new ProcessingResult(successfulReadings, failedReadings, errors);
        }
    }

}
}

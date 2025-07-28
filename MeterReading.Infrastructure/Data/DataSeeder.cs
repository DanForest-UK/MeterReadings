using CsvHelper;
using MeterReading.Domain;
using MeterReading.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterReading.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedTestAccountsAsync(MeterReadingContext context, string csvFilePath)
        {
            if (context.Accounts.Any())
                return;

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Map CSV directly to domain entities
            var accounts = csv.GetRecords<dynamic>().Select(record => new Account(
                new AccountId(int.Parse(record.AccountId)),
                new Person((string)record.FirstName, (string)record.LastName)
            ));

            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();
        }
    }
}

using CsvHelper;
using MeterReading.Infrastructure.Validation;
using System.Globalization;
using static LanguageExt.Prelude;

namespace MeterReading.Infrastructure.Services
{
    /// <summary>
    /// Validates CSV file headers and format
    /// </summary>
    public static class CsvHeaderValidator
    {
        /// <summary>
        /// Expected CSV headers
        /// </summary>
        static readonly string[] ExpectedHeaders = new[]
        {
            "AccountId",
            "MeterReadingDateTime",
            "MeterReadValue"
        };

        /// <summary>
        /// Validates that the CSV file has the correct headers and structure
        /// </summary>
        public static ValidationResult<string[]> ValidateHeaders(Stream csvStream)
        {
            try
            {
                csvStream.Position = 0;

                var reader = new StreamReader(csvStream, leaveOpen: true);
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture, leaveOpen: true);

                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null || csv.HeaderRecord.Length == 0)
                    return ValidationResult<string[]>.Failure("Invalid CSV format");

                if (csv.HeaderRecord.Length > ExpectedHeaders.Length)
                    return ValidationResult<string[]>.Failure("Invalid CSV format");

                var actualHeaders = (from h in csv.HeaderRecord
                                     let trimmed = h?.Trim()                                    
                                     select trimmed).ToArray();

                var missingHeaders = (from expected in ExpectedHeaders
                                      where !actualHeaders.Contains(expected, StringComparer.OrdinalIgnoreCase)
                                      select expected).ToArray();

                var extraHeaders = (from actual in actualHeaders
                                    where !ExpectedHeaders.Contains(actual, StringComparer.OrdinalIgnoreCase)
                                    select actual).ToArray();

                if (missingHeaders.Any() || extraHeaders.Any())
                    return ValidationResult<string[]>.Failure("Invalid CSV format");

                while (csv.Read())
                {
                    if (csv.Parser.Count > ExpectedHeaders.Length)
                    {
                        var extraValues = (from i in Enumerable.Range(ExpectedHeaders.Length, csv.Parser.Count - ExpectedHeaders.Length)
                                           let extraValue = csv.Parser[i]?.Trim()
                                           where !string.IsNullOrEmpty(extraValue)
                                           select unit).Any();

                        if (extraValues)
                            return ValidationResult<string[]>.Failure("Invalid CSV format");
                    }
                }

                return ValidationResult<string[]>.Success(actualHeaders);
            }
            catch (Exception ex)
            {
                return ValidationResult<string[]>.Failure("Invalid CSV format");
            }
            finally
            {
                csvStream.Position = 0;
            }
        }
    }
}

using CsvHelper;
using MeterReading.Infrastructure.Validation;
using System.Globalization;

namespace MeterReading.Infrastructure.Services
{
    /// <summary>
    /// Validates CSV file headers and format
    /// </summary>
    public static class CsvHeaderValidator
    {
        private static readonly string[] ExpectedHeaders = new[]
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
                // Reset stream position
                csvStream.Position = 0;

                // Create a StreamReader but don't dispose it (let the calling code handle disposal)
                var reader = new StreamReader(csvStream, leaveOpen: true);
                var csv = new CsvReader(reader, CultureInfo.InvariantCulture, leaveOpen: true);

                // Read headers
                csv.Read();
                csv.ReadHeader();

                if (csv.HeaderRecord == null || csv.HeaderRecord.Length == 0)
                {
                    return ValidationResult<string[]>.Failure("Invalid CSV format");
                }

                // Get all headers including empty ones to detect extra columns
                var rawHeaders = csv.HeaderRecord;

                // Check for extra columns (more than 3 expected)
                if (rawHeaders.Length > ExpectedHeaders.Length)
                {
                    return ValidationResult<string[]>.Failure("Invalid CSV format");
                }

                // Clean headers (trim and remove empty ones for comparison)
                var actualHeaders = rawHeaders
                    .Select(h => h?.Trim())
                    .Where(h => !string.IsNullOrEmpty(h))
                    .ToArray();

                // Check if we have the exact expected headers
                var missingHeaders = ExpectedHeaders.Except(actualHeaders, StringComparer.OrdinalIgnoreCase).ToArray();
                var extraHeaders = actualHeaders.Except(ExpectedHeaders, StringComparer.OrdinalIgnoreCase).ToArray();

                if (missingHeaders.Any() || extraHeaders.Any())
                {
                    return ValidationResult<string[]>.Failure("Invalid CSV format");
                }

                // Validate all data rows to check for extra values
                int rowNumber = 1; // Start at 1 since we've read the header

                while (csv.Read())
                {
                    rowNumber++;

                    // Check if this row has more fields than expected
                    if (csv.Parser.Count > ExpectedHeaders.Length)
                    {
                        var extraValues = new List<string>();
                        for (int i = ExpectedHeaders.Length; i < csv.Parser.Count; i++)
                        {
                            var extraValue = csv.Parser[i]?.Trim();
                            if (!string.IsNullOrEmpty(extraValue))
                            {
                                extraValues.Add($"'{extraValue}'");
                            }
                        }

                        if (extraValues.Any())
                        {
                            return ValidationResult<string[]>.Failure("Invalid CSV format");
                        }
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
                // Reset stream position for subsequent processing
                csvStream.Position = 0;
            }
        }
    }
}
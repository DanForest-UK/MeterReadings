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

        private static readonly string ExpectedFormat = @"Expected CSV format:
File must contain exactly 3 columns with headers: AccountId, MeterReadingDateTime, MeterReadValue
AccountId: Integer (must exist in system)
MeterReadingDateTime: Format dd/MM/yyyy HH:mm (e.g., 22/04/2019 09:24)  
MeterReadValue: Integer between 0 and 99999

Example:
AccountId,MeterReadingDateTime,MeterReadValue
2344,22/04/2019 09:24,1002
2233,22/04/2019 12:25,0323
8766,22/04/2019 12:25,3440";

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
                    return ValidationResult<string[]>.Failure(
                        $"CSV file has no headers.{Environment.NewLine}{Environment.NewLine}{ExpectedFormat}");
                }

                // Get all headers including empty ones to detect extra columns
                var rawHeaders = csv.HeaderRecord;

                // Check for extra columns (more than 3 expected)
                if (rawHeaders.Length > ExpectedHeaders.Length)
                {
                    var extraColumnCount = rawHeaders.Length - ExpectedHeaders.Length;
                    return ValidationResult<string[]>.Failure(
                        $"CSV file has {extraColumnCount} extra column(s). Expected exactly {ExpectedHeaders.Length} columns but found {rawHeaders.Length}.{Environment.NewLine}" +
                        $"Please remove extra columns and ensure your CSV has exactly 3 columns.{Environment.NewLine}{Environment.NewLine}{ExpectedFormat}");
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
                    var errorMessage = "CSV header validation failed:";

                    if (missingHeaders.Any())
                    {
                        errorMessage += $"{Environment.NewLine}• Missing headers: {string.Join(", ", missingHeaders)}";
                    }

                    if (extraHeaders.Any())
                    {
                        errorMessage += $"{Environment.NewLine}• Unexpected headers: {string.Join(", ", extraHeaders)}";
                    }

                    errorMessage += $"{Environment.NewLine}{Environment.NewLine}{ExpectedFormat}";

                    return ValidationResult<string[]>.Failure(errorMessage);
                }

                // Validate all data rows to check for extra values
                var dataRowErrors = new List<string>();
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
                            dataRowErrors.Add($"• Row {rowNumber}: {string.Join(", ", extraValues)}");
                        }
                    }
                }

                if (dataRowErrors.Any())
                {
                    var errorMessage = $"CSV contains extra data in {dataRowErrors.Count} row(s). Each row must have exactly 3 values.{Environment.NewLine}{Environment.NewLine}";
                    errorMessage += $"Rows with extra data:{Environment.NewLine}";
                    errorMessage += string.Join(Environment.NewLine, dataRowErrors);
                    errorMessage += $"{Environment.NewLine}{Environment.NewLine}Please remove extra columns or trailing commas.{Environment.NewLine}{Environment.NewLine}{ExpectedFormat}";

                    return ValidationResult<string[]>.Failure(errorMessage);
                }

                // Check header order (optional but recommended)
                var headerOrderCorrect = actualHeaders.Length == ExpectedHeaders.Length &&
                    actualHeaders.Zip(ExpectedHeaders, (actual, expected) =>
                        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                    .All(match => match);

                if (!headerOrderCorrect)
                {
                    var warningMessage = $"Headers are present but in different order. " +
                        $"Expected order: {string.Join(", ", ExpectedHeaders)}. " +
                        $"Actual order: {string.Join(", ", actualHeaders)}. " +
                        $"Processing will continue but consider reordering for consistency.";

                    // This is just a warning, still return success
                    Console.WriteLine($"Warning: {warningMessage}");
                }

                return ValidationResult<string[]>.Success(actualHeaders);
            }
            catch (Exception ex)
            {
                return ValidationResult<string[]>.Failure(
                    $"Error reading CSV file: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ExpectedFormat}");
            }
            finally
            {
                // Reset stream position for subsequent processing
                csvStream.Position = 0;
            }
        }
    }
}
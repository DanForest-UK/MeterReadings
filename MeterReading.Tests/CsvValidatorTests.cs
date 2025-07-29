using FluentAssertions;
using MeterReading.Infrastructure.Services;
using System.Text;
using Xunit;

namespace MeterReading.Tests
{
    /// <summary>
    /// Unit tests for the CsvHeaderValidator class.
    /// Tests CSV header validation functionality including format validation, header matching, and edge cases.
    /// </summary>
    public class CsvHeaderValidatorTests
    {
        /// <summary>
        /// Tests that a valid CSV with correct headers passes validation.
        /// Verifies that the validator correctly identifies and returns the expected headers.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithValidCsv()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new[] { "AccountId", "MeterReadingDateTime", "MeterReadValue" });
        }

        /// <summary>
        /// Tests that an empty CSV file fails validation.
        /// Verifies that the validator properly handles files with no content.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithNoHeaders()
        {
            var csvContent = "";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        /// <summary>
        /// Tests that a CSV with extra columns fails validation.
        /// Verifies that the validator rejects files with additional unexpected columns.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithExtraColumns()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue,ExtraColumn\n2344,22/04/2019 09:24,1002,extra";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        /// <summary>
        /// Tests that a CSV with missing required headers fails validation.
        /// Verifies that the validator rejects files that don't contain all expected columns.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithMissingHeaders()
        {
            var csvContent = "AccountId,MeterReadingDateTime\n2344,22/04/2019 09:24";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        /// <summary>
        /// Tests that a CSV with incorrect header names fails validation.
        /// Verifies that the validator enforces exact header name requirements (case-insensitive).
        /// </summary>
        [Fact]
        public void ValidateHeadersWithWrongHeaders()
        {
            var csvContent = "AccountNum,ReadingDate,Value\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        /// <summary>
        /// Tests that a CSV with extra data in rows (more values than headers) fails validation.
        /// Verifies that the validator detects and rejects malformed rows with additional data.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithExtraDataInRows()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002,extradata";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        /// <summary>
        /// Tests that headers with different case variations pass validation.
        /// Verifies that the validator performs case-insensitive header matching.
        /// </summary>
        [Fact]
        public void ValidateHeadersDifferentCase()
        {
            var csvContent = "accountid,meterreadingdatetime,meterreadvalue\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new[] { "accountid", "meterreadingdatetime", "meterreadvalue" });
        }

        /// <summary>
        /// Tests that headers with leading and trailing whitespace are properly trimmed and validated.
        /// Verifies that the validator handles whitespace gracefully and returns cleaned header names.
        /// </summary>
        [Fact]
        public void ValidateHeadersWithWhitespaceInHeaders()
        {
            var csvContent = " AccountId , MeterReadingDateTime , MeterReadValue \n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new[] { "AccountId", "MeterReadingDateTime", "MeterReadValue" });
        }
    }
}
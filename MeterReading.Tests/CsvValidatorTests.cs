using FluentAssertions;
using MeterReading.Infrastructure.Services;
using System.Text;
using Xunit;

namespace MeterReading.Tests
{
    public class CsvHeaderValidatorTests
    {
        [Fact]
        public void ValidateHeadersWithValidCsv()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new[] { "AccountId", "MeterReadingDateTime", "MeterReadValue" });
        }

        [Fact]
        public void ValidateHeadersWithNoHeaders()
        {
            var csvContent = "";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        [Fact]
        public void ValidateHeadersWithExtraColumns()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue,ExtraColumn\n2344,22/04/2019 09:24,1002,extra";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        [Fact]
        public void ValidateHeadersWithMissingHeaders()
        {
            var csvContent = "AccountId,MeterReadingDateTime\n2344,22/04/2019 09:24";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        [Fact]
        public void ValidateHeadersWithWrongHeaders()
        {
            var csvContent = "AccountNum,ReadingDate,Value\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        [Fact]
        public void ValidateHeadersWithExtraDataInRows()
        {
            var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002,extradata";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid CSV format");
        }

        [Fact]
        public void ValidateHeadersWithHeadersInDifferentCase()
        {
            var csvContent = "accountid,meterreadingdatetime,meterreadvalue\n2344,22/04/2019 09:24,1002";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            var result = CsvHeaderValidator.ValidateHeaders(stream);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(new[] { "accountid", "meterreadingdatetime", "meterreadvalue" });
        }

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
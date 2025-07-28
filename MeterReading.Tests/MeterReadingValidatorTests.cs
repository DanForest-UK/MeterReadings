using FluentAssertions;
using MeterReading.Domain;
using MeterReading.Infrastructure.Validation;
using Xunit;
using System;
using System.Collections.Generic;

namespace MeterReading.Tests
{
    public class MeterReadingValidatorTests
    {
        [Fact]
        public void ValidateAccountExistsSuccess()
        {
            var validIds = new HashSet<AccountId> { new AccountId(1) };
            var result = MeterReadingValidator.ValidateAccountExists("1", validIds);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(new AccountId(1));
        }

        [Fact]
        public void ValidateAccountExistsFail()
        {
            var result = MeterReadingValidator.ValidateAccountExists("", new());
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("empty");
        }

        [Fact]
        public void ValidateDateSuccess()
        {
            var input = "28/07/2025 14:30";
            var result = MeterReadingValidator.ValidateDateTime(input);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(DateTime.ParseExact(input, "dd/MM/yyyy HH:mm", null));
        }

        [Fact]
        public void ValidateDateFail()
        {
            var result = MeterReadingValidator.ValidateDateTime("invalid date");
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid datetime format");
        }

        [Fact]
        public void ValidateMeterReadingSuccess()
        {
            var result = MeterReadingValidator.ValidateMeterReadValue("99999");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(99999);
        }

        [Fact]
        public void ValidateMeterReadingRange()
        {
            var result = MeterReadingValidator.ValidateMeterReadValue("100000");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("between 0 and 99999");
        }

        [Fact]
        public void ValidatePersonNames()
        {
            var result = MeterReadingValidator.ValidatePersonName("  John ", "Doe");

            result.IsSuccess.Should().BeTrue();
            result.Value.FirstName.Should().Be("John");
            result.Value.LastName.Should().Be("Doe");
        }

        [Fact]
        public void ValidatePersonNameFail()
        {
            var result = MeterReadingValidator.ValidatePersonName("", "Doe");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("First name");

            result = MeterReadingValidator.ValidatePersonName("John", "");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Last name");
        }      
    }

}

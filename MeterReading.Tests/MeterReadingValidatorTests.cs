using FluentAssertions;
using MeterReading.Domain;
using MeterReading.Infrastructure.Data;
using MeterReading.Infrastructure.Validation;
using Xunit;
using System;
using System.Collections.Generic;

namespace MeterReading.Tests
{
    /// <summary>
    /// Unit tests for the MeterReadingValidator class.
    /// Tests validation functionality for meter reading data including account validation, 
    /// date/time parsing, meter value ranges, and person name constraints.
    /// </summary>
    public class MeterReadingValidatorTests
    {
        /// <summary>
        /// Tests that account validation succeeds when the account ID exists in the valid accounts set.
        /// Verifies that a valid account ID string is correctly parsed and validated against the account cache.
        /// </summary>
        [Fact]
        public void ValidateAccountExistsSuccess()
        {
            var validIds = new HashSet<AccountId> { new AccountId(1) };
            var result = MeterReadingValidator.ValidateAccountExists("1", validIds);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(new AccountId(1));
        }

        /// <summary>
        /// Tests that account validation fails when the account ID is empty or doesn't exist.
        /// Verifies proper error handling for invalid account scenarios.
        /// </summary>
        [Fact]
        public void ValidateAccountExistsFail()
        {
            var result = MeterReadingValidator.ValidateAccountExists("", new());
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("empty");
        }

        /// <summary>
        /// Tests that date/time validation succeeds with a properly formatted date string.
        /// Verifies that the expected format (dd/MM/yyyy HH:mm) is correctly parsed and converted to UTC.
        /// </summary>
        [Fact]
        public void ValidateDateSuccess()
        {
            var input = "28/07/2025 14:30";
            var result = MeterReadingValidator.ValidateDateTime(input);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(DateTime.ParseExact(input, "dd/MM/yyyy HH:mm", null));
        }

        /// <summary>
        /// Tests that date/time validation fails with an invalid date format.
        /// Verifies that improperly formatted date strings are rejected with appropriate error messages.
        /// </summary>
        [Fact]
        public void ValidateDateFail()
        {
            var result = MeterReadingValidator.ValidateDateTime("invalid date");
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Invalid datetime format");
        }

        /// <summary>
        /// Tests that meter reading value validation succeeds with a valid value within the allowed range.
        /// Verifies that valid numeric values (0-99999) are correctly parsed and accepted.
        /// </summary>
        [Fact]
        public void ValidateMeterReadingSuccess()
        {
            var result = MeterReadingValidator.ValidateMeterReadValue("99999");

            result.IsSuccess.Should().BeTrue();
            result.Value.Value.Should().Be(99999);
        }

        /// <summary>
        /// Tests that meter reading value validation fails when the value exceeds the allowed range.
        /// Verifies that values outside the 0-99999 range are rejected with appropriate error messages.
        /// </summary>
        [Fact]
        public void ValidateMeterReadingRange()
        {
            var result = MeterReadingValidator.ValidateMeterReadValue("100000");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("between 0 and 99999");
        }

        /// <summary>
        /// Tests that person name validation succeeds with valid names, including proper whitespace trimming.
        /// Verifies that valid first and last names are correctly processed and trimmed of leading/trailing spaces.
        /// </summary>
        [Fact]
        public void ValidatePersonNamesSuccess()
        {
            var result = MeterReadingValidator.ValidatePersonName("  John ", "Doe");

            result.IsSuccess.Should().BeTrue();
            result.Value.FirstName.Should().Be("John");
            result.Value.LastName.Should().Be("Doe");
        }

        /// <summary>
        /// Tests that person name validation fails when either first name or last name is empty.
        /// Verifies that both first and last names are required and cannot be empty strings.
        /// </summary>
        [Fact]
        public void ValidatePersonNameEmpty()
        {
            var result = MeterReadingValidator.ValidatePersonName("", "Doe");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("First name");

            result = MeterReadingValidator.ValidatePersonName("John", "");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Last name");
        }

        /// <summary>
        /// Tests that person name validation fails when names exceed the database column length limit.
        /// Verifies that names longer than MAX_NAME_LENGTH characters are rejected with detailed error messages
        /// including the actual length that caused the validation failure.
        /// </summary>
        [Fact]
        public void ValidatePersonNameTooLong()
        {
            var longFirstName = new string('A', MeterReadingContext.MAX_NAME_LENGTH + 1);
            var result = MeterReadingValidator.ValidatePersonName(longFirstName, "Doe");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"First name cannot exceed {MeterReadingContext.MAX_NAME_LENGTH} characters");
            result.ErrorMessage.Should().Contain($"Current length: {MeterReadingContext.MAX_NAME_LENGTH + 1}");

            var longLastName = new string('B', MeterReadingContext.MAX_NAME_LENGTH + 1);
            result = MeterReadingValidator.ValidatePersonName("John", longLastName);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain($"Last name cannot exceed {MeterReadingContext.MAX_NAME_LENGTH} characters");
            result.ErrorMessage.Should().Contain($"Current length: {MeterReadingContext.MAX_NAME_LENGTH + 1}");
        }

        /// <summary>
        /// Tests that person name validation succeeds when names are exactly at the maximum allowed length.
        /// Verifies that the boundary condition (exactly MAX_NAME_LENGTH characters) is handled correctly
        /// and such names are accepted as valid.
        /// </summary>
        [Fact]
        public void ValidatePersonNameExactlyMaxLength()
        {
            var maxLengthName = new string('A', MeterReadingContext.MAX_NAME_LENGTH);
            var result = MeterReadingValidator.ValidatePersonName(maxLengthName, maxLengthName);

            result.IsSuccess.Should().BeTrue();
            result.Value.FirstName.Should().Be(maxLengthName);
            result.Value.LastName.Should().Be(maxLengthName);
        }

        /// <summary>
        /// Tests that person name validation properly handles null values, empty strings, and whitespace-only strings.
        /// Verifies that all forms of "empty" input (null, empty string, whitespace) are consistently 
        /// rejected for both first and last names.
        /// </summary>
        /// <param name="name">The test input representing various forms of empty/invalid name values</param>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void ValidatePersonNameHandlesNullAndWhitespace(string name)
        {
            var result = MeterReadingValidator.ValidatePersonName(name, "Doe");
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("First name cannot be empty");

            result = MeterReadingValidator.ValidatePersonName("John", name);
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Last name cannot be empty");
        }
    }
}
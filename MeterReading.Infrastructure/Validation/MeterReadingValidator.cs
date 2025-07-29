using MeterReading.Domain;
using MeterReading.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using LanguageExt;
using Generic = System.Collections.Generic;

namespace MeterReading.Infrastructure.Validation
{
    /// <summary>
    /// Provides validation methods for meter reading data using domain types and database constraints
    /// </summary>
    public static class MeterReadingValidator
    {
        /// <summary>
        /// Validates that an account exists in the provided set of valid account IDs
        /// </summary>
        public static ValidationResult<AccountId> ValidateAccountExists(string accountId, Generic.HashSet<AccountId> validAccountIds) =>
            string.IsNullOrWhiteSpace(accountId)
                ? ValidationResult<AccountId>.Failure("Account ID is empty")
                : parseInt(accountId).Match(
                    Some: intAccountId =>
                        validAccountIds.Contains(new AccountId(intAccountId))
                            ? ValidationResult<AccountId>.Success(new AccountId(intAccountId))
                            : ValidationResult<AccountId>.Failure($"Account {accountId} does not exist"),
                    None: () => ValidationResult<AccountId>.Failure($"Account {accountId} is not a valid number"));

        /// <summary>
        /// Validates and parses a datetime string, ensuring UTC kind for PostgreSQL compatibility
        /// </summary>
        public static ValidationResult<DateTime> ValidateDateTime(string dateTimeString) =>
            string.IsNullOrWhiteSpace(dateTimeString)
                ? ValidationResult<DateTime>.Failure("DateTime is empty")
                : ParseDateTime(dateTimeString).Match(
                    Some: dt => ValidationResult<DateTime>.Success(DateTime.SpecifyKind(dt, DateTimeKind.Utc)),
                    None: () => ValidationResult<DateTime>.Failure($"Invalid datetime format: {dateTimeString}. Expected format: dd/MM/yyyy HH:mm"));

        /// <summary>
        /// Validates that a meter read value is within the allowed range 
        /// </summary>
        public static ValidationResult<MeterReadValue> ValidateMeterReadValue(string meterValue) =>
            string.IsNullOrWhiteSpace(meterValue)
                ? ValidationResult<MeterReadValue>.Failure("Meter read value is empty")
                : parseInt(meterValue).Match(
                    Some: intMeterValue =>
                        intMeterValue >= 0 && intMeterValue <= 99999
                            ? ValidationResult<MeterReadValue>.Success(new MeterReadValue(intMeterValue))
                            : ValidationResult<MeterReadValue>.Failure($"Meter read value must be between 0 and 99999. Got: {intMeterValue}"),
                    None: () => ValidationResult<MeterReadValue>.Failure($"Meter read value {meterValue} is not a valid number"));

        /// <summary>
        /// Validates that person name fields are not empty, within database length constraints, and returns domain type
        /// </summary>
        public static ValidationResult<Person> ValidatePersonName(string firstName, string lastName)
        {
            // Check for null or empty values
            if (string.IsNullOrWhiteSpace(firstName))
                return ValidationResult<Person>.Failure("First name cannot be empty");

            if (string.IsNullOrWhiteSpace(lastName))
                return ValidationResult<Person>.Failure("Last name cannot be empty");

            // Trim whitespace for validation
            var trimmedFirstName = firstName.Trim();
            var trimmedLastName = lastName.Trim();

            // Validate length constraints to match database schema
            if (trimmedFirstName.Length > MeterReadingContext.MAX_NAME_LENGTH)
                return ValidationResult<Person>.Failure($"First name cannot exceed {MeterReadingContext.MAX_NAME_LENGTH} characters. Current length: {trimmedFirstName.Length}");

            if (trimmedLastName.Length > MeterReadingContext.MAX_NAME_LENGTH)
                return ValidationResult<Person>.Failure($"Last name cannot exceed {MeterReadingContext.MAX_NAME_LENGTH} characters. Current length: {trimmedLastName.Length}");

            return ValidationResult<Person>.Success(new Person(trimmedFirstName, trimmedLastName));
        }

        /// <summary>
        /// Parses datetime string using the expected format
        /// </summary>
        /// <param name="dateTimeString">The datetime string to parse</param>
        /// <returns>Option containing parsed DateTime if successful, None if parsing fails</returns>
        static Option<DateTime> ParseDateTime(string dateTimeString) =>
            Try.lift(() =>
                DateTime.TryParseExact(dateTimeString, "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
                    ? Some(result)
                    : None).IfFail(default);
    }
}
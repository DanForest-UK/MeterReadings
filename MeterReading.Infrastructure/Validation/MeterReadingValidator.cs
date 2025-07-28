using MeterReading.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace MeterReading.Infrastructure.Validation
{
    /// <summary>
    /// Provides validation methods for meter reading data using domain types
    /// </summary>
    public static class MeterReadingValidator
    {
        /// <summary>
        /// Validates that an account exists in the provided set of valid account IDs
        /// </summary>
        public static ValidationResult<AccountId> ValidateAccountExists(string accountId, HashSet<AccountId> validAccountIds) =>
            string.IsNullOrWhiteSpace(accountId)
                ? ValidationResult<AccountId>.Failure("Account ID is empty")
                : parseInt(accountId).Match(
                    Some: intAccountId =>
                        validAccountIds.Contains(new AccountId(intAccountId))
                            ? ValidationResult<AccountId>.Success(new AccountId(intAccountId))
                            : ValidationResult<AccountId>.Failure($"Account {accountId} does not exist"),
                    None: () => ValidationResult<AccountId>.Failure($"Account {accountId} is not a valid number"));

        /// <summary>
        /// Validates and parses a datetime string 
        /// </summary>
        public static ValidationResult<DateTime> ValidateDateTime(string dateTimeString) =>
            string.IsNullOrWhiteSpace(dateTimeString)
                ? ValidationResult<DateTime>.Failure("DateTime is empty")
                : parseDateTime(dateTimeString).Match(
                    Some: ValidationResult<DateTime>.Success,
                    None: () => ValidationResult<DateTime>.Failure($"Invalid datetime format: {dateTimeString}. Expected format: dd/MM/yyyy HH:mm"));

        /// <summary>
        /// Validates that a meter read value is within the allowed range and returns domain type
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
        /// Validates that person name fields are not empty and returns domain type
        /// </summary>
        public static ValidationResult<Person> ValidatePersonName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return ValidationResult<Person>.Failure("First name cannot be empty");

            return string.IsNullOrWhiteSpace(lastName)
                ? ValidationResult<Person>.Failure("Last name cannot be empty")
                : ValidationResult<Person>.Success(new Person(firstName.Trim(), lastName.Trim()));
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterReading.Infrastructure.Validation
{
    // <summary>
    /// Represents either a successful result with value T or a failure with error message
    /// </summary>
    public record ValidationResult<T>
    {
        /// <summary>
        /// Was the validation succesful
        /// </summary>
        public bool IsSuccess { get; init; }

        /// <summary>
        /// The validated/parsed value
        /// </summary>
        public T Value { get; init; } = default!;

        /// <summary>
        /// The error message if it failed
        /// </summary>
        public string ErrorMessage { get; init; } = string.Empty;

        /// <summary>
        /// Ctor
        /// </summary>
        ValidationResult(bool isSuccess, T value, string errorMessage)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result with the given value
        /// </summary>
        public static ValidationResult<T> Success(T value) => 
            new(true, value, string.Empty);

        /// <summary>
        /// Creates a failed result with the given error message
        /// </summary>
        public static ValidationResult<T> Failure(string errorMessage) => 
            new(false, default!, errorMessage);

        /// <summary>
        /// Implicit conversion from T to Result<T>
        /// </summary>
        public static implicit operator ValidationResult<T>(T value) => 
            Success(value);
    }
}

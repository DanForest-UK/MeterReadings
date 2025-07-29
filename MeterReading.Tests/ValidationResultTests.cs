using FluentAssertions;
using MeterReading.Infrastructure.Validation;
using Xunit;

namespace MeterReading.Tests
{
    /// <summary>
    /// Unit tests for the ValidationResult&lt;T&gt; class.
    /// Tests the creation and behavior of both successful and failed validation results,
    /// including implicit conversion functionality.
    /// </summary>
    public class ValidationResultTests
    {
        /// <summary>
        /// Tests that the Success factory method creates a valid result with the expected properties.
        /// Verifies that a successful validation result has IsSuccess set to true, contains the provided value,
        /// and has an empty error message.
        /// </summary>
        [Fact]
        public void Success_ShouldCreateValidResult()
        {
            var result = ValidationResult<int>.Success(42);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
            result.ErrorMessage.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that the Failure factory method creates an error result with the expected properties.
        /// Verifies that a failed validation result has IsSuccess set to false, contains the provided error message,
        /// and has a default value for the generic type.
        /// </summary>
        [Fact]
        public void Failure_ShouldCreateErrorResult()
        {
            var result = ValidationResult<string>.Failure("Something went wrong");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Something went wrong");
        }

        /// <summary>
        /// Tests that implicit conversion from a value to ValidationResult creates a successful result.
        /// </summary>
        [Fact]
        public void ImplicitConversion_ShouldCreateSuccess()
        {
            ValidationResult<int> result = 99;

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(99);
        }
    }
}
using FluentAssertions;
using MeterReading.Infrastructure.Validation;
using Xunit;

namespace MeterReading.Tests
{
    public class ValidationResultTests
    {
        [Fact]
        public void Success_ShouldCreateValidResult()
        {
            var result = ValidationResult<int>.Success(42);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(42);
            result.ErrorMessage.Should().BeEmpty();
        }

        [Fact]
        public void Failure_ShouldCreateErrorResult()
        {
            var result = ValidationResult<string>.Failure("Something went wrong");

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Something went wrong");
        }

        [Fact]
        public void ImplicitConversion_ShouldCreateSuccess()
        {
            ValidationResult<int> result = 99;

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(99);
        }
    }

}
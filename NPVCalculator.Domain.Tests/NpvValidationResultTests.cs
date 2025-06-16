using FluentAssertions;
using NPVCalculator.Domain.Models;

namespace NPVCalculator.Domain.Tests
{
    public class NpvValidationResultTests
    {
        [Fact]
        public void IsValid_WithNoErrors_ShouldReturnTrue()
        {
            // Arrange
            var result = new NpvValidationResult();

            // Act & Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithErrors_ShouldReturnFalse()
        {
            // Arrange
            var result = new NpvValidationResult();
            result.AddError("Test error");

            // Act & Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void AddError_WithValidError_ShouldAddToCollection()
        {
            // Arrange
            var result = new NpvValidationResult();
            var errorMessage = "Test error message";

            // Act
            result.AddError(errorMessage);

            // Assert
            result.Errors.Should().Contain(errorMessage);
            result.Errors.Should().HaveCount(1);
        }

        [Fact]
        public void AddError_WithNullOrWhitespace_ShouldNotAdd()
        {
            // Arrange
            var result = new NpvValidationResult();

            // Act
            result.AddError(null!);
            result.AddError("");
            result.AddError("   ");

            // Assert
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void AddWarning_WithValidWarning_ShouldAddToCollection()
        {
            // Arrange
            var result = new NpvValidationResult();
            var warningMessage = "Test warning";

            // Act
            result.AddWarning(warningMessage);

            // Assert
            result.Warnings.Should().Contain(warningMessage);
            result.IsValid.Should().BeTrue(); // Warnings don't affect validity
        }

        [Fact]
        public void GetSummary_WithErrorsAndWarnings_ShouldReturnFormattedString()
        {
            // Arrange
            var result = new NpvValidationResult();
            result.AddError("Error 1");
            result.AddError("Error 2");
            result.AddWarning("Warning 1");

            // Act
            var summary = result.GetSummary();

            // Assert
            summary.Should().Contain("Errors: Error 1, Error 2");
            summary.Should().Contain("Warnings: Warning 1");
        }
    }
}

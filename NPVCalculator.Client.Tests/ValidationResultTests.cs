using FluentAssertions;
using NPVCalculator.Client.Models;
using System.ComponentModel.DataAnnotations;

namespace NPVCalculator.Client.Tests.Models
{
    public class ValidationResultTests
    {
        [Fact]
        public void IsValid_WithNoErrors_ShouldReturnTrue()
        {
            // Arrange
            var result = new InputValidationResult();

            // Act & Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithErrors_ShouldReturnFalse()
        {
            // Arrange
            var result = new InputValidationResult();
            result.Errors.Add("Test error");

            // Act & Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Constructor_ShouldInitializeEmptyErrorList()
        {
            // Act
            var result = new InputValidationResult();

            // Assert
            result.Errors.Should().NotBeNull();
            result.Errors.Should().BeEmpty();
        }
    }
}
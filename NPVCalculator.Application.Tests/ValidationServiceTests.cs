using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.Application.Services;
using NPVCalculator.Shared.Models;
using Xunit;

namespace NPVCalculator.Application.Tests
{
    public class ValidationServiceTests
    {
        private readonly Mock<ILogger<ValidationService>> _mockLogger;
        private readonly ValidationService _service;

        public ValidationServiceTests()
        {
            _mockLogger = new Mock<ILogger<ValidationService>>();
            _service = new ValidationService(_mockLogger.Object);
        }

        [Fact]
        public void ValidateNpvRequest_WithValidRequest_ShouldReturnValidResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _service.ValidateNpvRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateNpvRequest_WithNullRequest_ShouldReturnInvalidResult()
        {
            // Act
            var result = _service.ValidateNpvRequest(null);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Request cannot be null");
        }

        [Fact]
        public void ValidateNpvRequest_WithEmptyCashFlows_ShouldReturnInvalidResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal>(),
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _service.ValidateNpvRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("At least one cash flow is required");
        }

        [Fact]
        public void ValidateNpvRequest_WithInvalidRateRange_ShouldReturnInvalidResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 15m, 
                UpperBoundRate = 1m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _service.ValidateNpvRequest(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Upper bound must be greater than lower bound");
        }

        [Fact]
        public void ValidateNpvRequest_WithAllPositiveCashFlows_ShouldAddWarning()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { 1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _service.ValidateNpvRequest(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Warnings.Should().Contain("All cash flows are positive - unusual for NPV calculations");
        }
    }
}

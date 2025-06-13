using FluentAssertions;
using NPVCalculator.Client.Models;
using NPVCalculator.Client.Services;

namespace NPVCalculator.Client.Tests.Services
{
    public class InputValidationServiceTests
    {
        private readonly InputValidationService _validator;

        public InputValidationServiceTests()
        {
            _validator = new InputValidationService();
        }

        [Fact]
        public void ValidateInput_WithValidModel_ShouldReturnValidResult()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = "-1000,300,400,500",
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _validator.ValidateInput(model);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateInput_WithEmptyCashFlows_ShouldReturnInvalidResult()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = "",
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _validator.ValidateInput(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Cash flows cannot be empty");
        }

        [Fact]
        public void ValidateInput_WithInvalidCashFlowFormat_ShouldReturnInvalidResult()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = "-1000,abc,400",
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _validator.ValidateInput(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Invalid cash flows format");
        }

        [Fact]
        public void ValidateInput_WithWhitespaceCashFlows_ShouldReturnInvalidResult()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = "   ",
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var result = _validator.ValidateInput(model);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Cash flows cannot be empty");
        }
    }
}
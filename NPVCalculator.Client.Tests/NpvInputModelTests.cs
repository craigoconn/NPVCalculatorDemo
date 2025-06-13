using FluentAssertions;
using NPVCalculator.Client.Models;

namespace NPVCalculator.Client.Tests.Models
{
    public class NpvInputModelTests
    {
        [Fact]
        public void ToNpvRequest_WithValidCashFlows_ShouldParseCorrectly()
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
            var request = model.ToNpvRequest();

            // Assert
            request.CashFlows.Should().BeEquivalentTo(new[] { -1000m, 300m, 400m, 500m });
            request.LowerBoundRate.Should().Be(1m);
            request.UpperBoundRate.Should().Be(15m);
            request.RateIncrement.Should().Be(0.25m);
        }

        [Fact]
        public void ToNpvRequest_WithSpacesInCashFlows_ShouldTrimAndParse()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = " -1000 , 300 , 400 , 500 ",
                LowerBoundRate = 1m,
                UpperBoundRate = 15m,
                RateIncrement = 0.25m
            };

            // Act
            var request = model.ToNpvRequest();

            // Assert
            request.CashFlows.Should().BeEquivalentTo(new[] { -1000m, 300m, 400m, 500m });
        }

        [Fact]
        public void ToNpvRequest_WithInvalidCashFlows_ShouldThrowFormatException()
        {
            // Arrange
            var model = new NpvInputModel
            {
                CashFlowsInput = "-1000,abc,400,500"
            };

            // Act & Assert
            var action = () => model.ToNpvRequest();
            action.Should().Throw<FormatException>();
        }

        [Fact]
        public void Constructor_ShouldSetDefaultValues()
        {
            // Act
            var model = new NpvInputModel();

            // Assert
            model.CashFlowsInput.Should().Be("-1000,300,400,500");
            model.LowerBoundRate.Should().Be(1.00m);
            model.UpperBoundRate.Should().Be(15.00m);
            model.RateIncrement.Should().Be(0.25m);
            model.HasCashFlowError.Should().BeFalse();
        }
    }
}

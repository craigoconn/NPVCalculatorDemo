using FluentAssertions;
using NPVCalculator.Domain.Services;
using Xunit;

namespace NPVCalculator.Domain.Tests.Services
{
    public class NpvDomainServiceTests
    {
        private readonly NpvDomainService _service;

        public NpvDomainServiceTests()
        {
            _service = new NpvDomainService();
        }

        [Fact]
        public void CalculateNpv_WithValidInputs_ShouldReturnCorrectNpv()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 300, 400, 500 };
            var discountRate = 0.1m; // 10%

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-21.04m, 0.01m);
        }

        [Fact]
        public void CalculateNpv_WithZeroDiscountRate_ShouldReturnSumOfCashFlows()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 300, 400, 500 };
            var discountRate = 0m;

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(200m);
        }

        [Fact]
        public void CalculateNpv_WithNullCashFlows_ShouldThrowArgumentException()
        {
            // Arrange
            List<decimal> cashFlows = null;
            var discountRate = 0.1m;

            // Act & Assert
            var action = () => _service.CalculateNpv(cashFlows, discountRate);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Cash flows cannot be null or empty*")
                  .And.ParamName.Should().Be("cashFlows");
        }

        [Fact]
        public void CalculateNpv_WithEmptyCashFlows_ShouldThrowArgumentException()
        {
            // Arrange
            var cashFlows = new List<decimal>();
            var discountRate = 0.1m;

            // Act & Assert
            var action = () => _service.CalculateNpv(cashFlows, discountRate);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Cash flows cannot be null or empty*")
                  .And.ParamName.Should().Be("cashFlows");
        }

        [Fact]
        public void CalculateNpv_WithSingleCashFlow_ShouldReturnThatValue()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000 };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(-1000m);
        }

        [Fact]
        public void CalculateNpv_WithHighDiscountRate_ShouldReturnCorrectValue()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 1100 };
            var discountRate = 0.5m; // 50%

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-266.67m, 0.01m);
        }

        [Fact]
        public void CalculateNpv_WithNegativeDiscountRate_ShouldHandleCorrectly()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 500 };
            var discountRate = -0.1m;

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-444.44m, 0.01m);
        }

        [Fact]
        public void CalculateNpv_ShouldRoundToTwoDecimalPlaces()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 333.333m };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(Math.Round(result, 2));
            result.ToString().Split('.').LastOrDefault()?.Length.Should().BeLessThanOrEqualTo(2);
        }
    }
}
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.Application.Services;
using NPVCalculator.Shared.Models;
using Xunit;

namespace NPVCalculator.Application.Tests
{
    public class NpvCalculatorServiceTests
    {
        private readonly Mock<ILogger<NpvCalculatorService>> _mockLogger;
        private readonly NpvCalculatorService _service;

        public NpvCalculatorServiceTests()
        {
            _mockLogger = new Mock<ILogger<NpvCalculatorService>>();
            _service = new NpvCalculatorService(_mockLogger.Object);
        }

        #region CalculateSingleNpv Tests

        [Fact]
        public void CalculateSingleNpv_WithValidInputs_ShouldReturnCorrectNpv()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 300, 400, 500 };
            var discountRate = 0.1m; // 10%

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-21.04m, 0.01m);
        }

        [Fact]
        public void CalculateSingleNpv_WithZeroDiscountRate_ShouldReturnSumOfCashFlows()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 300, 400, 500 };
            var discountRate = 0m;

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(200m); 
        }

        [Fact]
        public void CalculateSingleNpv_WithNullCashFlows_ShouldThrowArgumentException()
        {
            // Arrange
            List<decimal> cashFlows = null;
            var discountRate = 0.1m;

            // Act & Assert
            var action = () => _service.CalculateSingleNpv(cashFlows, discountRate);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Cash flows cannot be null or empty*")
                  .And.ParamName.Should().Be("cashFlows");
        }

        [Fact]
        public void CalculateSingleNpv_WithEmptyCashFlows_ShouldThrowArgumentException()
        {
            // Arrange
            var cashFlows = new List<decimal>();
            var discountRate = 0.1m;

            // Act & Assert
            var action = () => _service.CalculateSingleNpv(cashFlows, discountRate);
            action.Should().Throw<ArgumentException>()
                  .WithMessage("Cash flows cannot be null or empty*")
                  .And.ParamName.Should().Be("cashFlows");
        }

        [Fact]
        public void CalculateSingleNpv_WithSingleCashFlow_ShouldReturnThatValue()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000 };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(-1000m);
        }

        [Fact]
        public void CalculateSingleNpv_WithHighDiscountRate_ShouldReturnCorrectValue()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 1100 };
            var discountRate = 0.5m; // 50%

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-266.67m, 0.01m);
        }

        [Fact]
        public void CalculateSingleNpv_WithNegativeDiscountRate_ShouldHandleCorrectly()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 500 };
            var discountRate = -0.1m; 

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-444.44m, 0.01m); 
        }

        #endregion

        #region CalculateAsync Tests

        [Fact]
        public async Task CalculateAsync_WithValidRequest_ShouldReturnCorrectNumberOfResults()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(5);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 2m, 3m, 4m, 5m });
        }

        [Fact]
        public async Task CalculateAsync_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = async () => await _service.CalculateAsync(null);
            await action.Should().ThrowAsync<ArgumentNullException>()
                        .WithParameterName("request");
        }

        [Fact]
        public async Task CalculateAsync_WithSingleRate_ShouldReturnOneResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 5m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(1);
            results.First().Rate.Should().Be(5m);
        }

        [Fact]
        public async Task CalculateAsync_WithFractionalIncrement_ShouldReturnCorrectResults()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 500, 600 },
                LowerBoundRate = 1m,
                UpperBoundRate = 2m,
                RateIncrement = 0.5m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(3);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 1.5m, 2m });
        }

        [Fact]
        public async Task CalculateAsync_WithManyRates_ShouldYieldPeriodically()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 100, 200, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 25m,
                RateIncrement = 1m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(25);
            results.All(r => r.Value != 0).Should().BeTrue();
        }

        [Fact]
        public async Task CalculateAsync_ShouldLogInformationMessages()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 3m,
                RateIncrement = 1m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(3);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting NPV calculation for 3 rates")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("NPV calculation completed with 3 results")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Calculate (Synchronous) Tests

        [Fact]
        public void Calculate_WithValidRequest_ShouldReturnCorrectNumberOfResults()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(5);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 2m, 3m, 4m, 5m });
        }

        [Fact]
        public void Calculate_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => _service.Calculate(null);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("request");
        }

        [Fact]
        public void Calculate_WithSingleRate_ShouldReturnOneResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 10m,
                UpperBoundRate = 10m,
                RateIncrement = 1m
            };

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(1);
            results.First().Rate.Should().Be(10m);
        }

        [Fact]
        public void Calculate_WithDecimalIncrement_ShouldHandleCorrectly()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 500, 600 },
                LowerBoundRate = 0.5m,
                UpperBoundRate = 1.5m,
                RateIncrement = 0.25m
            };

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(5); 
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 0.5m, 0.75m, 1m, 1.25m, 1.5m });
        }

        [Fact]
        public void Calculate_WithLargeIncrement_ShouldStopAtUpperBound()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 500, 600 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 3m
            };

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(2);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 4m });
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvCalculatorService(null);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("logger");
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        public void CalculateSingleNpv_WithZeroCashFlows_ShouldReturnZero()
        {
            // Arrange
            var cashFlows = new List<decimal> { 0, 0, 0 };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(0m);
        }

        [Fact]
        public void CalculateSingleNpv_WithLargeCashFlows_ShouldHandleCorrectly()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000000, 500000, 600000 };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().BeApproximately(-49586.78m, 0.01m);
        }

        [Fact]
        public async Task CalculateAsync_WithVerySmallIncrement_ShouldHandleCorrectly()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 1.02m,
                RateIncrement = 0.01m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(3);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 1.01m, 1.02m });
        }

        [Fact]
        public void Calculate_WithRoundingIssues_ShouldHandleFloatingPointPrecision()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 0.1m,
                UpperBoundRate = 0.3m,
                RateIncrement = 0.1m
            };

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(3); 
            results.All(r => r.Rate >= 0.1m && r.Rate <= 0.3m).Should().BeTrue();
        }

        [Fact]
        public void CalculateSingleNpv_ShouldRoundToTwoDecimalPlaces()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 333.333m };
            var discountRate = 0.1m;

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(Math.Round(result, 2));
            result.ToString().Split('.').LastOrDefault()?.Length.Should().BeLessThanOrEqualTo(2);
        }

        [Fact]
        public async Task CalculateAsync_WithExactUpperBoundMatch_ShouldIncludeUpperBound()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 3m,
                RateIncrement = 1m
            };

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(3);
            results.Last().Rate.Should().Be(3m);
        }

        #endregion
    }
}
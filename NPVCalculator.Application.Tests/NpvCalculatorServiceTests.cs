using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.Application.Services;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using Xunit;

namespace NPVCalculator.Application.Tests
{
    public class NpvCalculatorServiceTests
    {
        private readonly Mock<INpvDomainService> _mockNpvDomainService;
        private readonly Mock<ILogger<NpvCalculatorService>> _mockLogger;
        private readonly NpvCalculatorService _service;

        public NpvCalculatorServiceTests()
        {
            _mockNpvDomainService = new Mock<INpvDomainService>();
            _mockLogger = new Mock<ILogger<NpvCalculatorService>>();
            _service = new NpvCalculatorService(_mockNpvDomainService.Object, _mockLogger.Object);
        }

        [Fact]
        public void CalculateSingleNpv_ShouldDelegateToDomainService()
        {
            // Arrange
            var cashFlows = new List<decimal> { -1000, 300, 400, 500 };
            var discountRate = 0.1m;
            var expectedNpv = 150.25m;

            _mockNpvDomainService.Setup(x => x.CalculateNpv(cashFlows, discountRate))
                               .Returns(expectedNpv);

            // Act
            var result = _service.CalculateSingleNpv(cashFlows, discountRate);

            // Assert
            result.Should().Be(expectedNpv);
            _mockNpvDomainService.Verify(x => x.CalculateNpv(cashFlows, discountRate), Times.Once);
        }

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

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(100m);

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(5);
            results.Select(r => r.Rate).Should().BeEquivalentTo(new[] { 1m, 2m, 3m, 4m, 5m });

            _mockNpvDomainService.Verify(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()),
                                       Times.Exactly(5));
        }

        [Fact]
        public async Task CalculateAsync_WithNullRequest_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = async () => await _service.CalculateAsync(null!);
            await action.Should().ThrowAsync<ArgumentNullException>()
                        .WithParameterName("request");
        }

        [Fact]
        public async Task CalculateAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 100m, 
                RateIncrement = 1m
            };

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(100m);

            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            // Act & Assert
            var action = async () => await _service.CalculateAsync(request, cancellationTokenSource.Token);
            await action.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task CalculateAsync_WithAlreadyCancelledToken_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 50m,
                RateIncrement = 1m
            };

            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(100m);

            // Act & Assert
            var action = async () => await _service.CalculateAsync(request, cancellationTokenSource.Token);
            await action.Should().ThrowAsync<OperationCanceledException>();

            _mockNpvDomainService.Verify(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()),
                                       Times.Never);
        }

        [Fact]
        public async Task CalculateAsync_WithValidCancellationToken_ShouldCompleteSuccessfully()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 3m,
                RateIncrement = 1m
            };

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(75m);

            using var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var results = await _service.CalculateAsync(request, cancellationTokenSource.Token);

            // Assert
            results.Should().HaveCount(3);
            _mockNpvDomainService.Verify(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()),
                                       Times.Exactly(3));
        }

        [Fact]
        public void Calculate_WithValidRequest_ShouldDelegateToDomainService()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 3m,
                RateIncrement = 1m
            };

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(75m);

            // Act
            var results = _service.Calculate(request);

            // Assert
            results.Should().HaveCount(3);
            _mockNpvDomainService.Verify(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()),
                                       Times.Exactly(3));
        }

        [Fact]
        public void Constructor_WithNullNpvDomainService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvCalculatorService(null!, _mockLogger.Object);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("npvDomainService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvCalculatorService(_mockNpvDomainService.Object, null!);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("logger");
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

            _mockNpvDomainService.Setup(x => x.CalculateNpv(It.IsAny<IList<decimal>>(), It.IsAny<decimal>()))
                               .Returns(100m);

            // Act
            var results = await _service.CalculateAsync(request);

            // Assert
            results.Should().HaveCount(3);
        }
    }
}
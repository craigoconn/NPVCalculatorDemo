using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.Application.Services;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Domain.Models;
using NPVCalculator.Shared.Models;
using Xunit;

namespace NPVCalculator.Application.Tests
{
    public class NpvApplicationServiceTests
    {
        private readonly Mock<INpvCalculatorService> _mockCalculator;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly Mock<ILogger<NpvApplicationService>> _mockLogger;
        private readonly NpvApplicationService _service;

        public NpvApplicationServiceTests()
        {
            _mockCalculator = new Mock<INpvCalculatorService>();
            _mockValidationService = new Mock<IValidationService>();
            _mockLogger = new Mock<ILogger<NpvApplicationService>>();
            _service = new NpvApplicationService(
                _mockCalculator.Object,
                _mockValidationService.Object,
                _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act & Assert
            _service.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullCalculator_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var action = () => new NpvApplicationService(
                null!,
                _mockValidationService.Object,
                _mockLogger.Object);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("calculator");
        }

        [Fact]
        public void Constructor_WithNullValidationService_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var action = () => new NpvApplicationService(
                _mockCalculator.Object,
                null!,
                _mockLogger.Object);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("validationService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Arrange & Act
            var action = () => new NpvApplicationService(
                _mockCalculator.Object,
                _mockValidationService.Object,
                null!);

            // Assert
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("logger");
        }

        #endregion

        #region Success Scenarios

        [Fact]
        public async Task ProcessCalculationAsync_WithValidRequest_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var expectedResults = CreateSampleNpvResults();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _service.ProcessCalculationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(expectedResults);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithValidRequestAndWarnings_ShouldReturnSuccessWithWarnings()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidationResultWithWarnings();
            var expectedResults = CreateSampleNpvResults();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _service.ProcessCalculationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(expectedResults);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().Contain("Test warning");
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithCancellationToken_ShouldPassTokenToCalculator()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var expectedResults = CreateSampleNpvResults();
            var cancellationToken = new CancellationToken();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, cancellationToken))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _service.ProcessCalculationAsync(request, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            _mockCalculator.Verify(x => x.CalculateAsync(request, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithEmptyResults_ShouldReturnSuccessWithEmptyResults()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var emptyResults = new List<NpvResult>();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(emptyResults);

            // Act
            var result = await _service.ProcessCalculationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region Validation Failure Scenarios

        [Fact]
        public async Task ProcessCalculationAsync_WithValidationErrors_ShouldReturnValidationFailure()
        {
            // Arrange
            var request = CreateInvalidNpvRequest();
            var validationResult = CreateInvalidValidationResult();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain("Cash flows cannot be empty");
            result.Errors.Should().Contain("Discount rate must be positive");
            result.Warnings.Should().BeEmpty();

            // Verify calculator was not called
            _mockCalculator.Verify(x => x.CalculateAsync(It.IsAny<NpvRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithValidationErrorsAndWarnings_ShouldReturnFailureWithWarnings()
        {
            // Arrange
            var request = CreateInvalidNpvRequest();
            var validationResult = CreateInvalidValidationResultWithWarnings();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Errors.Should().Contain("Cash flows cannot be empty");
            result.Warnings.Should().Contain("Large discount rate detected");
        }

        #endregion

        #region Exception Handling Scenarios

        [Fact]
        public async Task ProcessCalculationAsync_WhenValidationServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var expectedException = new InvalidOperationException("Validation service failed");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ProcessCalculationAsync(request));

            exception.Message.Should().Be("Validation service failed");
        }

        [Fact]
        public async Task ProcessCalculationAsync_WhenCalculatorThrows_ShouldPropagateException()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var expectedException = new InvalidOperationException("Calculator service failed");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ProcessCalculationAsync(request));

            exception.Message.Should().Be("Calculator service failed");
        }

        [Fact]
        public async Task ProcessCalculationAsync_WhenOperationCancelled_ShouldPropagateOperationCancelledException()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var cancellationToken = new CancellationToken(true); // Already cancelled

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, cancellationToken))
                          .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _service.ProcessCalculationAsync(request, cancellationToken));
        }

        #endregion

        #region Logging Tests

        [Fact]
        public async Task ProcessCalculationAsync_WithValidRequest_ShouldLogInformationMessages()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var expectedResults = CreateSampleNpvResults();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResults);

            // Act
            await _service.ProcessCalculationAsync(request);

            // Assert
            VerifyLogCalled(LogLevel.Information, "Processing NPV calculation request with 4 cash flows");
            VerifyLogCalled(LogLevel.Information, "NPV calculation completed with 2 results");
        }

        [Fact]
        public async Task ProcessCalculationAsync_WhenExceptionOccurs_ShouldLogError()
        {
            // Arrange
            var request = CreateValidNpvRequest();
            var validationResult = CreateValidValidationResult();
            var expectedException = new InvalidOperationException("Test exception");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(expectedException);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ProcessCalculationAsync(request));

            VerifyLogCalled(LogLevel.Error, "Error in NPV calculation processing", expectedException);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithNullCashFlows_ShouldLogZeroCashFlows()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = null,
                LowerBoundRate = 5m,
                UpperBoundRate = 10m,
                RateIncrement = 1m
            };
            var validationResult = CreateInvalidValidationResult();

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);

            // Act
            await _service.ProcessCalculationAsync(request);

            // Assert
            VerifyLogCalled(LogLevel.Information, "Processing NPV calculation request with 0 cash flows");
        }

        #endregion

        #region Helper Methods

        private static NpvRequest CreateValidNpvRequest()
        {
            return new NpvRequest
            {
                CashFlows = new List<decimal> { -1000m, 300m, 400m, 500m },
                LowerBoundRate = 5m,
                UpperBoundRate = 10m,
                RateIncrement = 2.5m
            };
        }

        private static NpvRequest CreateInvalidNpvRequest()
        {
            return new NpvRequest
            {
                CashFlows = new List<decimal>(),
                LowerBoundRate = -5m,
                UpperBoundRate = 10m,
                RateIncrement = 1m
            };
        }

        private static NpvValidationResult CreateValidValidationResult()
        {
            return new NpvValidationResult();
        }

        private static NpvValidationResult CreateValidationResultWithWarnings()
        {
            var result = new NpvValidationResult();
            result.AddWarning("Test warning");
            return result;
        }

        private static NpvValidationResult CreateInvalidValidationResult()
        {
            var result = new NpvValidationResult();
            result.AddError("Cash flows cannot be empty");
            result.AddError("Discount rate must be positive");
            return result;
        }

        private static NpvValidationResult CreateInvalidValidationResultWithWarnings()
        {
            var result = new NpvValidationResult();
            result.AddError("Cash flows cannot be empty");
            result.AddWarning("Large discount rate detected");
            return result;
        }

        private static List<NpvResult> CreateSampleNpvResults()
        {
            return new List<NpvResult>
            {
                new() { Rate = 5m, Value = 110.15m },
                new() { Rate = 7.5m, Value = 45.32m }
            };
        }

        private void VerifyLogCalled(LogLevel level, string message)
        {
            _mockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void VerifyLogCalled(LogLevel level, string message, Exception exception)
        {
            _mockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
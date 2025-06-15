using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.API.Controllers;
using NPVCalculator.Domain.Entities;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.API.Tests
{
    public class NpvControllerTests
    {
        private readonly Mock<INpvCalculator> _mockCalculator;
        private readonly Mock<IValidationService> _mockValidationService;
        private readonly Mock<ILogger<NpvController>> _mockLogger;
        private readonly NpvController _controller;

        public NpvControllerTests()
        {
            _mockCalculator = new Mock<INpvCalculator>();
            _mockValidationService = new Mock<IValidationService>();
            _mockLogger = new Mock<ILogger<NpvController>>();
            _controller = new NpvController(_mockCalculator.Object, _mockValidationService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Calculate_WithValidRequest_ShouldReturnOkResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            var expectedResults = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m },
                new() { Rate = 2m, Value = 50m }
            };

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Calculate_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new NpvRequest();
            var validationResult = new NpvValidationResult();
            validationResult.AddError("Test error");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Calculate_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Calculate((NpvRequest)null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().NotBeNull();

            var response = badRequestResult!.Value;
            response.Should().BeEquivalentTo(new { success = false, errors = new[] { "Request body is required" } });
        }

        [Fact]
        public async Task Calculate_WhenArgumentExceptionThrown_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            var argumentException = new ArgumentException("Invalid cash flow values");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(argumentException);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new
            {
                success = false,
                errors = new[] { "Invalid cash flow values" }
            });
        }

        [Fact]
        public async Task Calculate_WhenUnexpectedExceptionThrown_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            var unexpectedException = new InvalidOperationException("Database connection failed");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(unexpectedException);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new
            {
                success = false,
                errors = new[] { "An error occurred while calculating NPV" }
            });
        }

        [Fact]
        public async Task Calculate_WhenOperationCancelled_ShouldReturn409Conflict()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            var cancellationException = new OperationCanceledException("Operation was cancelled");

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ThrowsAsync(cancellationException);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(409);
            objectResult.Value.Should().BeEquivalentTo(new
            {
                success = false,
                errors = new[] { "Operation was cancelled" }
            });
        }

        [Fact]
        public async Task Calculate_WithCancellationToken_ShouldPassTokenToCalculator()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            var expectedResults = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m }
            };
            using var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, cancellationToken))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.Calculate(request, cancellationToken);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCalculator.Verify(x => x.CalculateAsync(request, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Calculate_WithValidRequestAndWarnings_ShouldReturnOkWithWarnings()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new NpvValidationResult();
            validationResult.AddWarning("Warning: Large cash flow values detected");
            validationResult.AddWarning("Warning: Wide rate range specified");

            var expectedResults = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m },
                new() { Rate = 2m, Value = 50m }
            };

            _mockValidationService.Setup(x => x.ValidateNpvRequest(request))
                                  .Returns(validationResult);
            _mockCalculator.Setup(x => x.CalculateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedResults);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            var expectedResponse = new
            {
                success = true,
                data = expectedResults,
                warnings = new[] { "Warning: Large cash flow values detected", "Warning: Wide rate range specified" }
            };
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public void Constructor_WithNullCalculator_ShouldThrowArgumentNullException()
        {
            // FIXED: Cast null to the expected interface type
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new NpvController((INpvCalculator)null!, _mockValidationService.Object, _mockLogger.Object));

            exception.ParamName.Should().Be("calculator");
        }
        [Fact]
        public void Constructor_WithNullValidationService_ShouldThrowArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new NpvController(_mockCalculator.Object, null!, _mockLogger.Object));

            exception.ParamName.Should().Be("validationService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new NpvController(_mockCalculator.Object, _mockValidationService.Object, null!));

            exception.ParamName.Should().Be("logger");
        }

        [Fact]
        public void Health_ShouldReturnOkResult()
        {
            // Act
            var result = _controller.Health();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
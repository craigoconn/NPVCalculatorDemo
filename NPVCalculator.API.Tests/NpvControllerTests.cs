using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NPVCalculator.API.Controllers;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Application.Models;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.API.Tests
{
    public class NpvControllerTests
    {
        private readonly Mock<INpvApplicationService> _mockApplicationService;
        private readonly Mock<ILogger<NpvController>> _mockLogger;
        private readonly NpvController _controller;

        public NpvControllerTests()
        {
            _mockApplicationService = new Mock<INpvApplicationService>();
            _mockLogger = new Mock<ILogger<NpvController>>();
            _controller = new NpvController(_mockApplicationService.Object, _mockLogger.Object);
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

            var expectedResults = new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m },
                new() { Rate = 2m, Value = 50m }
            };

            var applicationResult = NpvApplicationResult.Success(expectedResults, new List<string> { "Test warning" });

            _mockApplicationService.Setup(x => x.ProcessCalculationAsync(request, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(applicationResult);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            var expectedResponse = new
            {
                success = true,
                data = expectedResults,
                warnings = new[] { "Test warning" }
            };
            okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Calculate_WithValidationFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new NpvRequest();
            var errors = new List<string> { "Cash flows are required", "Invalid rate range" };
            var warnings = new List<string> { "Test warning" };

            var applicationResult = NpvApplicationResult.ValidationFailure(errors, warnings);

            _mockApplicationService.Setup(x => x.ProcessCalculationAsync(request, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(applicationResult);

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            var expectedResponse = new
            {
                success = false,
                errors = errors.ToArray(),
                warnings = warnings.ToArray()
            };
            badRequestResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Calculate_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var result = await _controller.Calculate((NpvRequest)null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            var expectedResponse = new
            {
                success = false,
                errors = new[] { "Request body is required" }
            };
            badRequestResult!.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Calculate_WithOperationCancelledException_ShouldReturnConflict()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            _mockApplicationService.Setup(x => x.ProcessCalculationAsync(request, It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new OperationCanceledException("Calculation was cancelled"));

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(409);

            var expectedResponse = new
            {
                success = false,
                errors = new[] { "Operation was cancelled" }
            };
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task Calculate_WithUnexpectedException_ShouldReturnInternalServerError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            _mockApplicationService.Setup(x => x.ProcessCalculationAsync(request, It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act
            var result = await _controller.Calculate(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);

            var expectedResponse = new
            {
                success = false,
                errors = new[] { "An error occurred while calculating NPV" }
            };
            objectResult.Value.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public void Constructor_WithNullApplicationService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new NpvController(null!, _mockLogger.Object));

            exception.ParamName.Should().Be("applicationService");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new NpvController(_mockApplicationService.Object, null!));

            exception.ParamName.Should().Be("logger");
        }

        [Fact]
        public void Health_ShouldReturnOkResult()
        {
            // Act
            var result = _controller.Health();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();

            // Check that it has status and timestamp properties
            var value = okResult.Value;
            value.Should().BeEquivalentTo(new { status = "healthy" }, options => options.ExcludingMissingMembers());
        }
    }
}
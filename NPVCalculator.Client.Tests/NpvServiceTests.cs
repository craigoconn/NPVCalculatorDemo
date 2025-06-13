using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NPVCalculator.Client.Services;
using NPVCalculator.Shared.Models;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NPVCalculator.Client.Tests.Services
{
    public class NpvServiceTests
    {
        private readonly Mock<ILogger<NpvService>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly NpvService _service;

        public NpvServiceTests()
        {
            _mockLogger = new Mock<ILogger<NpvService>>();
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://localhost:7191/")
            };
            _service = new NpvService(_httpClient, _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithSuccessfulResponse_ShouldReturnSuccessResult()
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

            var responseContent = JsonSerializer.Serialize(new
            {
                success = true,
                data = expectedResults,
                warnings = new string[] { }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(expectedResults);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithErrorResponse_ShouldReturnErrorResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var errorResponse = JsonSerializer.Serialize(new
            {
                success = false,
                errors = new[] { "Invalid cash flows", "Rate increment too large" }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain("Invalid cash flows");
            result.Errors.Should().Contain("Rate increment too large");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithNetworkError_ShouldReturnNetworkErrorResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Network error");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithTimeout_ShouldReturnTimeoutErrorResult()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("timed out");
        }

        // NEW MISSING TESTS

        [Fact]
        public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvService(null, _mockLogger.Object);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvService(_httpClient, null);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("logger");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithNullRequest_ShouldStillMakeApiCall()
        {
            // Arrange
            NpvRequest request = null;

            var responseContent = JsonSerializer.Serialize(new
            {
                success = false,
                errors = new[] { "Request body is required" }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain("Request body is required");

            // Verify logging with null request
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling NPV API with 0 cash flows")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithSuccessfulResponseAndWarnings_ShouldReturnWarningsAsErrors()
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
                new() { Rate = 1m, Value = 100m }
            };

            var responseContent = JsonSerializer.Serialize(new
            {
                success = true,
                data = expectedResults,
                warnings = new[] { "Large cash flow detected", "Wide rate range specified" }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(expectedResults);
            result.Errors.Should().Contain("Warning: Large cash flow detected");
            result.Errors.Should().Contain("Warning: Wide rate range specified");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithSuccessfulResponseButNullData_ShouldReturnEmptyList()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var responseContent = JsonSerializer.Serialize(new
            {
                success = true,
                data = (List<NpvResult>)null,
                warnings = new string[] { }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task CalculateNpvAsync_WithInvalidJsonInSuccessResponse_ShouldReturnParseError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Unable to parse API response");

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to parse API response")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithSuccessResponseButSuccessFalse_ShouldReturnParseError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var responseContent = JsonSerializer.Serialize(new
            {
                success = false,
                data = new List<NpvResult>(),
                warnings = new string[] { }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Unable to parse API response");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithInvalidJsonInErrorResponse_ShouldReturnGenericError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid request data");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithInternalServerError_ShouldReturnServerError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal server error", Encoding.UTF8, "text/plain")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Server error occurred");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithUnknownStatusCode_ShouldReturnGenericApiError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var httpResponse = new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("Forbidden", Encoding.UTF8, "text/plain")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("API error: Forbidden");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithErrorResponseButNoErrors_ShouldReturnGenericError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var errorResponse = JsonSerializer.Serialize(new
            {
                success = false,
                errors = new string[] { }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Invalid request data");
        }

        [Fact]
        public async Task CalculateNpvAsync_WithUnexpectedException_ShouldReturnUnexpectedError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("Unexpected error"));

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("An unexpected error occurred.");

            // Verify error logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected error calling NPV API")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithTaskCanceledExceptionWithInnerException_ShouldReturnTimeoutError()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var innerException = new TimeoutException("Request timeout");
            var taskCanceledException = new TaskCanceledException("Request was canceled", innerException);

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(taskCanceledException);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("timed out");

            // Verify warning logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request timeout or cancellation")),
                    taskCanceledException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CalculateNpvAsync_WithRequestContainingNullCashFlows_ShouldLogCorrectly()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = null,
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var errorResponse = JsonSerializer.Serialize(new
            {
                success = false,
                errors = new[] { "Cash flows are required" }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorResponse, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeFalse();

            // Verify logging handles null cash flows correctly
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling NPV API with 0 cash flows")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CalculateNpvAsync_ShouldLogCorrectCashFlowCount()
        {
            // Arrange
            var request = new NpvRequest
            {
                CashFlows = new List<decimal> { -1000, 300, 400, 500, 600 },
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };

            var responseContent = JsonSerializer.Serialize(new
            {
                success = true,
                data = new List<NpvResult>(),
                warnings = new string[] { }
            });

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            _mockHttpHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _service.CalculateNpvAsync(request);

            // Assert
            result.IsSuccess.Should().BeTrue();

            // Verify correct cash flow count is logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling NPV API with 5 cash flows")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
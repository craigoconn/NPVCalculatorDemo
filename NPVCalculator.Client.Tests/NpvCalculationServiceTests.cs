using FluentAssertions;
using Moq;
using NPVCalculator.Client.Interfaces;
using NPVCalculator.Client.Models;
using NPVCalculator.Client.Services;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Tests.Services
{
    public class NpvCalculationServiceTests
    {
        private readonly Mock<INpvService> _mockNpvService;
        private readonly Mock<IInputValidationService> _mockInputValidator;
        private readonly NpvCalculationService _service;

        public NpvCalculationServiceTests()
        {
            _mockNpvService = new Mock<INpvService>();
            _mockInputValidator = new Mock<IInputValidationService>();
            _service = new NpvCalculationService(_mockNpvService.Object, _mockInputValidator.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullNpvService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvCalculationService(null!, _mockInputValidator.Object);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("npvService");
        }

        [Fact]
        public void Constructor_WithNullInputValidator_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new NpvCalculationService(_mockNpvService.Object, null!);
            action.Should().Throw<ArgumentNullException>()
                  .WithParameterName("inputValidator");
        }

        #endregion

        #region Success Scenarios

        [Fact]
        public async Task ProcessCalculationAsync_WithValidInput_ShouldReturnSuccessResult()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var validationResult = new InputValidationResult();
            var expectedResults = CreateSampleNpvResults();
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = true,
                Data = expectedResults
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Results.Should().BeEquivalentTo(expectedResults);
            result.Errors.Should().BeEmpty();
            result.ResultType.Should().Be(NpvCalculationResultType.Success);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithValidInputAndNullResults_ShouldReturnSuccessWithNullData()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var validationResult = new InputValidationResult();
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = true,
                Data = null
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Results.Should().BeNull();
            result.Errors.Should().BeEmpty();
            result.ResultType.Should().Be(NpvCalculationResultType.Success);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithInvalidInput_ShouldReturnValidationFailure()
        {
            // Arrange
            var inputModel = CreateInvalidInputModel();
            var validationErrors = new List<string> { "Cash flows are required", "Invalid rate range" };
            var validationResult = new InputValidationResult
            {
                Errors = validationErrors 
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().BeEquivalentTo(validationErrors);
            result.ResultType.Should().Be(NpvCalculationResultType.ValidationFailure);

            _mockNpvService.Verify(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithSingleValidationError_ShouldReturnValidationFailure()
        {
            // Arrange
            var inputModel = CreateInvalidInputModel();
            var validationResult = new InputValidationResult
            {
                Errors = new List<string> { "Single validation error" }
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain("Single validation error");
            result.ResultType.Should().Be(NpvCalculationResultType.ValidationFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithServiceFailure_ShouldReturnServiceFailure()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var validationResult = new InputValidationResult ();
            var serviceErrors = new List<string> { "Network error", "Server unavailable" };
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = false,
                Errors = serviceErrors
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().BeEquivalentTo(serviceErrors);
            result.ResultType.Should().Be(NpvCalculationResultType.ServiceFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithServiceFailureAndNullErrors_ShouldReturnEmptyErrors()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var validationResult = new InputValidationResult();
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = false,
                Errors = null!
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.ResultType.Should().Be(NpvCalculationResultType.ServiceFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithValidationException_ShouldReturnExceptionFailure()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var expectedException = new InvalidOperationException("Validation service failed");

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Throws(expectedException);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().Contain("Error: Validation service failed");
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithServiceException_ShouldReturnExceptionFailure()
        {
            // Arrange
            var inputModel = CreateValidInputModel();
            var validationResult = new InputValidationResult ();
            var expectedException = new HttpRequestException("Network connection failed");

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ThrowsAsync(expectedException);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().Contain("Error: Network connection failed");
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithInvalidCashFlowFormat_ShouldReturnExceptionFailure()
        {
            // Arrange
            var inputModel = new NpvInputModel
            {
                CashFlowsInput = "invalid,not-a-number,abc",
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new InputValidationResult();

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Should().StartWith("Error:");
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithWhitespaceInCashFlows_ShouldHandleCorrectly()
        {
            // Arrange
            var inputModel = new NpvInputModel
            {
                CashFlowsInput = " -1000 , 300 , 400 , 500 ",
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new InputValidationResult();
            var expectedResults = CreateSampleNpvResults();
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = true,
                Data = expectedResults
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.IsAny<NpvRequest>()))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Results.Should().BeEquivalentTo(expectedResults);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithEmptyCashFlowsInput_ShouldReturnExceptionFailure()
        {
            // Arrange
            var inputModel = new NpvInputModel
            {
                CashFlowsInput = "", 
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
            var validationResult = new InputValidationResult();

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
        }

        [Fact]
        public async Task ProcessCalculationAsync_WithNullInputModel_ShouldHandleGracefully()
        {
            // Arrange
            NpvInputModel inputModel = null!;
            var expectedException = new ArgumentNullException(nameof(inputModel));

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Throws(expectedException);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Should().StartWith("Error:");
        }

        [Fact]
        public async Task ProcessCalculationAsync_ToNpvRequestConversion_ShouldWorkCorrectly()
        {
            // Arrange
            var inputModel = new NpvInputModel
            {
                CashFlowsInput = "-2000,500,600,700",
                LowerBoundRate = 2m,
                UpperBoundRate = 8m,
                RateIncrement = 2m
            };
            var validationResult = new InputValidationResult();
            var expectedResults = CreateSampleNpvResults();
            var apiResponse = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = true,
                Data = expectedResults
            };

            _mockInputValidator.Setup(x => x.ValidateInput(inputModel))
                              .Returns(validationResult);
            _mockNpvService.Setup(x => x.CalculateNpvAsync(It.Is<NpvRequest>(req =>
                req.CashFlows.Count == 4 &&
                req.CashFlows[0] == -2000 &&
                req.LowerBoundRate == 2m &&
                req.UpperBoundRate == 8m &&
                req.RateIncrement == 2m)))
                          .ReturnsAsync(apiResponse);

            // Act
            var result = await _service.ProcessCalculationAsync(inputModel);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockNpvService.Verify(x => x.CalculateNpvAsync(It.Is<NpvRequest>(req =>
                req.CashFlows.SequenceEqual(new List<decimal> { -2000, 500, 600, 700 }) &&
                req.LowerBoundRate == 2m &&
                req.UpperBoundRate == 8m &&
                req.RateIncrement == 2m)), Times.Once);
        }

        [Fact]
        public void NpvCalculationResult_Success_ShouldCreateSuccessResult()
        {
            // Arrange
            var results = CreateSampleNpvResults();

            // Act
            var result = NpvCalculationResult.Success(results);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Results.Should().BeEquivalentTo(results);
            result.Errors.Should().BeEmpty();
            result.ResultType.Should().Be(NpvCalculationResultType.Success);
        }

        [Fact]
        public void NpvCalculationResult_ValidationFailure_ShouldCreateValidationFailureResult()
        {
            // Arrange
            var errors = new List<string> { "Error 1", "Error 2" };

            // Act
            var result = NpvCalculationResult.ValidationFailure(errors);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().BeEquivalentTo(errors);
            result.ResultType.Should().Be(NpvCalculationResultType.ValidationFailure);
        }

        [Fact]
        public void NpvCalculationResult_ServiceFailure_ShouldCreateServiceFailureResult()
        {
            // Arrange
            var errors = new List<string> { "Service Error 1", "Service Error 2" };

            // Act
            var result = NpvCalculationResult.ServiceFailure(errors);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().BeEquivalentTo(errors);
            result.ResultType.Should().Be(NpvCalculationResultType.ServiceFailure);
        }

        [Fact]
        public void NpvCalculationResult_ServiceFailureWithNullErrors_ShouldCreateEmptyErrorsList()
        {
            // Act
            var result = NpvCalculationResult.ServiceFailure(null);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().BeEmpty();
            result.ResultType.Should().Be(NpvCalculationResultType.ServiceFailure);
        }

        [Fact]
        public void NpvCalculationResult_ExceptionFailure_ShouldCreateExceptionFailureResult()
        {
            // Arrange
            var errorMessage = "Something went wrong";

            // Act
            var result = NpvCalculationResult.ExceptionFailure(errorMessage);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Results.Should().BeNull();
            result.Errors.Should().Contain("Error: Something went wrong");
            result.ResultType.Should().Be(NpvCalculationResultType.ExceptionFailure);
        }


        private static NpvInputModel CreateValidInputModel()
        {
            return new NpvInputModel
            {
                CashFlowsInput = "-1000,300,400,500", 
                LowerBoundRate = 1m,
                UpperBoundRate = 5m,
                RateIncrement = 1m
            };
        }

        private static NpvInputModel CreateInvalidInputModel()
        {
            return new NpvInputModel
            {
                CashFlowsInput = "", // Empty string = invalid
                LowerBoundRate = 5m,
                UpperBoundRate = 1m, // Upper < Lower = invalid
                RateIncrement = 0m // Zero increment = invalid
            };
        }

        private static List<NpvResult> CreateSampleNpvResults()
        {
            return new List<NpvResult>
            {
                new() { Rate = 1m, Value = 100m },
                new() { Rate = 2m, Value = 50m },
                new() { Rate = 3m, Value = 10m }
            };
        }

        #endregion
    }
}
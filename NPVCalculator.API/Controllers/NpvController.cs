using Microsoft.AspNetCore.Mvc;
using NPVCalculator.Domain.Entities;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using System.Net;

namespace NPVCalculator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NpvController : ControllerBase
    {
        private readonly INpvCalculator _calculator;
        private readonly IValidationService _validationService;
        private readonly ILogger<NpvController> _logger;

        public NpvController(INpvCalculator calculator, IValidationService validationService, ILogger<NpvController> logger)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] NpvRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return BadRequest(CreateErrorResponse("Request body is required"));

            try
            {
                _logger.LogInformation("NPV calculation request with {CashFlowCount} cash flows",
                    request.CashFlows?.Count ?? 0);

                var validation = _validationService.ValidateNpvRequest(request);
                if (!validation.IsValid)
                    return BadRequest(CreateValidationResponse(validation));

                var result = await _calculator.CalculateAsync(request, cancellationToken); 

                _logger.LogInformation("NPV calculation completed with {ResultCount} results", result.Count());
                return Ok(CreateSuccessResponse(result, validation.Warnings));
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex,"NPV calculation was cancelled");
                return StatusCode(409, CreateErrorResponse("Operation was cancelled"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in NPV calculation");
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in NPV calculation");
                return StatusCode(500, CreateErrorResponse("An error occurred while calculating NPV"));
            }
        }
        private object CreateErrorResponse(string error) =>
            new { success = false, errors = new[] { error } };

        private object CreateValidationResponse(NpvValidationResult validation) =>
            new { success = false, errors = validation.Errors.ToArray(), warnings = validation.Warnings.ToArray() };

        private object CreateSuccessResponse(IEnumerable<NpvResult> data, IList<string> warnings) =>
            new { success = true, data, warnings = warnings.ToArray() };

        [HttpGet("health")]
        [ProducesResponseType(typeof(OkObjectResult), (int)HttpStatusCode.OK)]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
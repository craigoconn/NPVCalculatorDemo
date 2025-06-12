using Microsoft.AspNetCore.Mvc;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;

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
        public async Task<IActionResult> Calculate([FromBody] NpvRequest request)
        {
            try
            {
                _logger.LogInformation("Received NPV calculation request with {CashFlowCount} cash flows",
                    request?.CashFlows?.Count ?? 0);

                var validation = _validationService.ValidateNpvRequest(request);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("NPV calculation request validation failed: {Errors}",
                        string.Join(", ", validation.Errors));

                    return BadRequest(new
                    {
                        success = false,
                        errors = validation.Errors,
                        warnings = validation.Warnings
                    });
                }

                var result = await _calculator.CalculateAsync(request);

                _logger.LogInformation("NPV calculation completed successfully with {ResultCount} results",
                    result.Count());

                return Ok(new
                {
                    success = true,
                    data = result,
                    warnings = validation.Warnings
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument provided for NPV calculation");
                return BadRequest(new
                {
                    success = false,
                    errors = new[] { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during NPV calculation");
                return StatusCode(500, new
                {
                    success = false,
                    errors = new[] { "An error occurred while calculating NPV" }
                });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
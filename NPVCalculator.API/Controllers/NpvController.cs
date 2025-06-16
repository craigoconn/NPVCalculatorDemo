using Microsoft.AspNetCore.Mvc;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NpvController : ControllerBase
    {
        private readonly INpvApplicationService _applicationService;
        private readonly ILogger<NpvController> _logger;

        public NpvController(INpvApplicationService applicationService, ILogger<NpvController> logger)
        {
            _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] NpvRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                return BadRequest(new { success = false, errors = new[] { "Request body is required" } });

            try
            {
                var result = await _applicationService.ProcessCalculationAsync(request, cancellationToken);

                return result.IsSuccess
                    ? Ok(new { success = true, data = result.Data, warnings = result.Warnings.ToArray() })
                    : BadRequest(new { success = false, errors = result.Errors.ToArray(), warnings = result.Warnings.ToArray() });
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "NPV calculation was cancelled");
                return StatusCode(409, new { success = false, errors = new[] { "Operation was cancelled" } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in NPV calculation");
                return StatusCode(500, new { success = false, errors = new[] { "An error occurred while calculating NPV" } });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
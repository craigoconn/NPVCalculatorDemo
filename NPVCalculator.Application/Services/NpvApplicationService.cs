using Microsoft.Extensions.Logging;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Application.Models;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Application.Services
{
    public class NpvApplicationService : INpvApplicationService
    {
        private readonly INpvCalculatorService _calculator;
        private readonly IValidationService _validationService;
        private readonly ILogger<NpvApplicationService> _logger;

        public NpvApplicationService(
            INpvCalculatorService calculator,
            IValidationService validationService,
            ILogger<NpvApplicationService> logger)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<NpvApplicationResult> ProcessCalculationAsync(NpvRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing NPV calculation request with {CashFlowCount} cash flows",
                    request.CashFlows?.Count ?? 0);

                var validation = _validationService.ValidateNpvRequest(request);
                if (!validation.IsValid)
                {
                    return NpvApplicationResult.ValidationFailure(validation.Errors, validation.Warnings);
                }

                var results = await _calculator.CalculateAsync(request, cancellationToken);

                _logger.LogInformation("NPV calculation completed with {ResultCount} results", results.Count());

                return NpvApplicationResult.Success(results, validation.Warnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NPV calculation processing");
                throw;
            }
        }
    }
}
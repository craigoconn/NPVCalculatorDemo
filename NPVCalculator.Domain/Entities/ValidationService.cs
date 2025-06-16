using Microsoft.Extensions.Logging;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Domain.Models;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        private const decimal MaxUpperBoundRate = 1000m;
        private const decimal MinLowerBoundRate = -100m;
        private const decimal MinRateIncrement = 0.01m;
        private const int MaxCalculations = 10000;
        private const int MaxCashFlows = 1000;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public NpvValidationResult ValidateNpvRequest(NpvRequest request)
        {
            var result = new NpvValidationResult();

            if (request == null)
            {
                result.AddError("Request cannot be null");
                return result;
            }

            ValidateCashFlows(request.CashFlows, result);
            ValidateDiscountRates(request.LowerBoundRate, request.UpperBoundRate, request.RateIncrement, result);
            ValidateSpecificRules(request, result);

            if (!result.IsValid)
            {
                _logger.LogWarning("Validation failed with {ErrorCount} errors", result.Errors.Count);
            }

            return result;
        }

        private static void ValidateCashFlows(IList<decimal> cashFlows, NpvValidationResult result)
        {
            if (cashFlows == null)
            {
                result.AddError("Cash flows cannot be null");
                return;
            }

            if (!cashFlows.Any())
            {
                result.AddError("At least one cash flow is required");
                return;
            }

            if (cashFlows.Count > MaxCashFlows)
            {
                result.AddError($"Too many cash flows. Maximum allowed: {MaxCashFlows}");
                return;
            }

            if (cashFlows.Any(cf => Math.Abs(cf) > 1_000_000_000_000m))
            {
                result.AddError("Cash flows contain extremely large values");
            }
        }

        private static void ValidateDiscountRates(decimal lowerBound, decimal upperBound, decimal increment, NpvValidationResult result)
        {
            if (lowerBound < MinLowerBoundRate)
                result.AddError($"Lower bound rate cannot be less than {MinLowerBoundRate}%");

            if (upperBound > MaxUpperBoundRate)
                result.AddError($"Upper bound rate cannot exceed {MaxUpperBoundRate}%");

            if (upperBound <= lowerBound)
                result.AddError("Upper bound must be greater than lower bound");

            if (increment < MinRateIncrement)
                result.AddError($"Rate increment must be at least {MinRateIncrement}%");
        }

        private static void ValidateSpecificRules(NpvRequest request, NpvValidationResult result)
        {
            var totalCalculations = (request.UpperBoundRate - request.LowerBoundRate) / request.RateIncrement;
            if (totalCalculations > MaxCalculations)
            {
                result.AddError($"Too many calculations ({totalCalculations:F0}). Maximum: {MaxCalculations}");
            }

            if (request.RateIncrement > (request.UpperBoundRate - request.LowerBoundRate))
            {
                result.AddError("Rate increment cannot be larger than the rate range");
            }

            if (request.CashFlows != null)
            {
                if (request.CashFlows.TrueForAll(cf => cf >= 0))
                {
                    result.AddWarning("All cash flows are positive - unusual for NPV calculations");
                }

                if (request.CashFlows.Any() && request.CashFlows[0] > 0)
                {
                    result.AddWarning("First cash flow is positive - typically initial investment is negative");
                }
            }
        }
    }
}
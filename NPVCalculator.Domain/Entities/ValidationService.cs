// Application/Services/ValidationService.cs
using NPVCalculator.Domain.Entities;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using Microsoft.Extensions.Logging;

namespace NPVCalculator.Application.Services
{
    /// <summary>
    /// Enhanced validation service following SOLID principles
    /// Single Responsibility: Only handles validation logic
    /// Open/Closed: Can be extended with new validation rules
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        // Configuration constants - could be moved to configuration
        private const decimal MaxUpperBoundRate = 1000m; // 1000%
        private const decimal MinLowerBoundRate = -100m; // -100%
        private const decimal MinRateIncrement = 0.01m; // 0.01%
        private const int MaxCalculations = 10000;
        private const int MaxCashFlows = 1000;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public NpvValidationResult ValidateNpvRequest(NpvRequest request)
        {
            var result = new NpvValidationResult();

            _logger.LogDebug("Starting validation for NPV request");

            if (request == null)
            {
                result.AddError("Request cannot be null");
                _logger.LogWarning("NPV request validation failed: null request");
                return result;
            }

            // Validate cash flows
            if (!ValidateCashFlows(request.CashFlows))
            {
                result.AddError("Invalid cash flows provided");
            }

            // Validate discount rates
            if (!ValidateDiscountRates(request.LowerBoundRate, request.UpperBoundRate, request.RateIncrement))
            {
                result.AddError("Invalid discount rate parameters");
            }

            // Additional specific validations
            ValidateSpecificRules(request, result);

            if (!result.IsValid)
            {
                _logger.LogWarning("NPV request validation failed with {ErrorCount} errors: {Errors}",
                    result.Errors.Count, string.Join("; ", result.Errors));
            }
            else
            {
                _logger.LogDebug("NPV request validation passed");
            }

            return result;
        }

        public bool ValidateCashFlows(IList<decimal> cashFlows)
        {
            if (cashFlows == null)
            {
                _logger.LogDebug("Cash flows validation failed: null");
                return false;
            }

            if (!cashFlows.Any())
            {
                _logger.LogDebug("Cash flows validation failed: empty collection");
                return false;
            }

            if (cashFlows.Count > MaxCashFlows)
            {
                _logger.LogDebug("Cash flows validation failed: too many cash flows ({Count} > {Max})",
                    cashFlows.Count, MaxCashFlows);
                return false;
            }

            // For decimal, we don't need to check for NaN or Infinity since decimal doesn't support them
            // Just verify all values are reasonable for financial calculations
            if (cashFlows.Any(cf => Math.Abs(cf) > 1_000_000_000_000m)) // 1 trillion limit
            {
                _logger.LogDebug("Cash flows validation failed: contains extremely large values");
                return false;
            }

            return true;
        }

        public bool ValidateDiscountRates(decimal lowerBound, decimal upperBound, decimal increment)
        {
            var isValid = true;

            if (lowerBound < MinLowerBoundRate)
            {
                _logger.LogDebug("Lower bound rate validation failed: {LowerBound} < {Min}", lowerBound, MinLowerBoundRate);
                isValid = false;
            }

            if (upperBound > MaxUpperBoundRate)
            {
                _logger.LogDebug("Upper bound rate validation failed: {UpperBound} > {Max}", upperBound, MaxUpperBoundRate);
                isValid = false;
            }

            if (upperBound <= lowerBound)
            {
                _logger.LogDebug("Discount rate range validation failed: upper bound ({Upper}) <= lower bound ({Lower})",
                    upperBound, lowerBound);
                isValid = false;
            }

            if (increment < MinRateIncrement)
            {
                _logger.LogDebug("Rate increment validation failed: {Increment} < {Min}", increment, MinRateIncrement);
                isValid = false;
            }

            return isValid;
        }

        private void ValidateSpecificRules(NpvRequest request, NpvValidationResult result)
        {
            // Check for reasonable number of calculations
            var totalCalculations = (request.UpperBoundRate - request.LowerBoundRate) / request.RateIncrement;
            if (totalCalculations > MaxCalculations)
            {
                result.AddError($"Too many calculations requested ({totalCalculations:F0}). Maximum allowed: {MaxCalculations}");
            }

            // Validate that increment makes sense relative to the range
            var range = request.UpperBoundRate - request.LowerBoundRate;
            if (request.RateIncrement > range)
            {
                result.AddError("Rate increment cannot be larger than the rate range");
            }

            // Business rule: Warn if all cash flows are positive (unusual for NPV)
            if (request.CashFlows != null && request.CashFlows.All(cf => cf >= 0))
            {
                _logger.LogInformation("Warning: All cash flows are non-negative, which is unusual for NPV calculations");
            }

            // Business rule: Warn if first cash flow is positive (usually initial investment is negative)
            if (request.CashFlows != null && request.CashFlows.Any() && request.CashFlows[0] > 0)
            {
                _logger.LogInformation("Warning: First cash flow is positive, typically initial investment is negative");
            }
        }
    }
}
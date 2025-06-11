using NPVCalculator.Domain.Entities;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Interfaces
{
    /// <summary>
    /// Service for validating NPV calculation requests
    /// Follows Single Responsibility Principle
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates an NPV calculation request
        /// </summary>
        NpvValidationResult ValidateNpvRequest(NpvRequest request);

        /// <summary>
        /// Validates individual cash flows
        /// </summary>
        bool ValidateCashFlows(IList<decimal> cashFlows);

        /// <summary>
        /// Validates discount rate parameters
        /// </summary>
        bool ValidateDiscountRates(decimal lowerBound, decimal upperBound, decimal increment);
    }
}

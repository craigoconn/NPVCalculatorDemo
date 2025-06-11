using NPVCalculator.Shared.Models; // Instead of Domain.Entities

namespace NPVCalculator.Domain.Interfaces
{
    /// <summary>
    /// Core business interface for NPV calculations
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface INpvCalculator
    {
        /// <summary>
        /// Calculate NPV for a single discount rate
        /// </summary>
        decimal CalculateSingleNpv(IList<decimal> cashFlows, decimal discountRate);

        /// <summary>
        /// Calculate NPV for multiple discount rates asynchronously
        /// </summary>
        Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request);

        /// <summary>
        /// Calculate NPV for multiple discount rates synchronously (for unit testing)
        /// </summary>
        IEnumerable<NpvResult> Calculate(NpvRequest request);
    }
}
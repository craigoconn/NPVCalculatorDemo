using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Interfaces
{
    public interface INpvCalculator
    {
        decimal CalculateSingleNpv(IList<decimal> cashFlows, decimal discountRate);
        Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request);
    }
}
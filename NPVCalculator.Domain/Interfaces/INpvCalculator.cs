using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Interfaces
{
    public interface INpvCalculator
    {
        Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request, CancellationToken cancellationToken = default);
        IEnumerable<NpvResult> Calculate(NpvRequest request);
        decimal CalculateSingleNpv(IList<decimal> cashFlows, decimal discountRate);
    }
}
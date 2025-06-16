using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Interfaces
{
    public interface INpvCalculatorService
    {
        Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request, CancellationToken cancellationToken = default);
    }
}
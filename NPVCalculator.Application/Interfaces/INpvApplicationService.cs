using NPVCalculator.Application.Models;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Application.Interfaces
{
    public interface INpvApplicationService
    {
        Task<NpvApplicationResult> ProcessCalculationAsync(NpvRequest request, CancellationToken cancellationToken);
    }
}

using NPVCalculator.Client.Models;
using NPVCalculator.Client.Services;

namespace NPVCalculator.Client.Interfaces
{
    public interface INpvCalculationService
    {
        Task<NpvCalculationResult> ProcessCalculationAsync(NpvInputModel inputModel);
    }

}
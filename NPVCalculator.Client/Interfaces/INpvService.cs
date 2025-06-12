using NPVCalculator.Client.Models;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Interfaces
{
    public interface INpvService
    {
        Task<ApiResponse<List<NpvResult>>> CalculateNpvAsync(NpvRequest request);
    }
}

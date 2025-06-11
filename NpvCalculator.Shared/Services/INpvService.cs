using NPVCalculator.Shared.Models;

namespace NpvCalculator.Shared.Services
{
    public interface INpvService
    {
        Task<ApiResponse<List<NpvResult>>> CalculateNpvAsync(NpvRequest request);
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}

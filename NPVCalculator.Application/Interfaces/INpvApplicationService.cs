using NPVCalculator.Shared.Models;

namespace NPVCalculator.Application.Interfaces
{
    /// <summary>
    /// </summary>
    public interface INpvApplicationService
    {
        /// <summary>
        /// Processes NPV calculation
        /// </summary>
        /// <param name="request">NPV calculation request</param>
        /// <returns>Calculation results</returns>
        Task<NpvCalculationResponse> ProcessNpvCalculationAsync(NpvRequest request);
    }

    /// <summary>
    /// Response object
    /// </summary>
    public class NpvCalculationResponse
    {
        public bool IsSuccess { get; set; }
        public IEnumerable<NpvResult>? Results { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Application.Models
{
    public class NpvApplicationResult
    {
        public bool IsSuccess { get; set; }
        public IEnumerable<NpvResult>? Data { get; set; }
        public List<string> Errors { get; set; } = [];
        public List<string> Warnings { get; set; } = [];

        public static NpvApplicationResult Success(IEnumerable<NpvResult> data, IList<string>? warnings = null)
        {
            return new NpvApplicationResult
            {
                IsSuccess = true,
                Data = data,
                Warnings = warnings?.ToList() ?? []
            };
        }

        public static NpvApplicationResult ValidationFailure(List<string> errors, List<string>? warnings = null)
        {
            return new NpvApplicationResult
            {
                IsSuccess = false,
                Errors = errors,
                Warnings = warnings ?? []
            };
        }
    }
}
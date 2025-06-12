using NPVCalculator.Domain.Entities;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Domain.Interfaces
{
    public interface IValidationService
    {
        NpvValidationResult ValidateNpvRequest(NpvRequest request);
    }
}
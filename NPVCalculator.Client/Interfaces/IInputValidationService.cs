using NPVCalculator.Client.Models;

namespace NPVCalculator.Client.Interfaces
{
    public interface IInputValidationService
    {
        InputValidationResult ValidateInput(NpvInputModel model);
    }
}
using NPVCalculator.Client.Interfaces;
using NPVCalculator.Client.Models;

namespace NPVCalculator.Client.Services
{
    public class InputValidationService : IInputValidationService
    {
        public InputValidationResult ValidateInput(NpvInputModel model)
        {
            var result = new InputValidationResult();

            if (string.IsNullOrWhiteSpace(model.CashFlowsInput))
            {
                result.Errors.Add("Cash flows cannot be empty");
                return result;
            }

            try
            {
                model.CashFlowsInput
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => decimal.Parse(s.Trim()))
                    .ToList();
            }
            catch
            {
                result.Errors.Add("Invalid cash flows format");
            }

            return result;
        }
    }
}

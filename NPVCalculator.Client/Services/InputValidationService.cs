using NPVCalculator.Client.Interfaces;
using NPVCalculator.Client.Models;

namespace NPVCalculator.Client.Services
{
    public class InputValidationService : IInputValidationService
    {
        public InputValidationResult ValidateInput(NpvInputModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var result = new InputValidationResult();

            ValidateCashFlows(model.CashFlowsInput, result);
            ValidateRates(model, result);

            return result;
        }

        private static void ValidateCashFlows(string? cashFlowsInput, InputValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(cashFlowsInput))
            {
                result.Errors.Add("Cash flows cannot be empty");
                return;
            }

            var tokens = cashFlowsInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (tokens.Length == 0)
            {
                result.Errors.Add("At least one cash flow is required");
                return;
            }

            var invalidEntries = new List<string>();
            var validCount = 0;

            foreach (var token in tokens)
            {
                if (decimal.TryParse(token, out _))
                {
                    validCount++;
                }
                else
                {
                    invalidEntries.Add(token);
                }
            }

            if (invalidEntries.Count > 0)
            {
                result.Errors.Add($"Invalid cash flow values: {string.Join(", ", invalidEntries)}");
            }

            if (validCount == 0)
            {
                result.Errors.Add("No valid cash flows found");
            }
        }

        private static void ValidateRates(NpvInputModel model, InputValidationResult result)
        {
            if (model.UpperBoundRate <= model.LowerBoundRate)
            {
                result.Errors.Add("Upper bound rate must be greater than lower bound rate");
            }

            if (model.RateIncrement <= 0)
            {
                result.Errors.Add("Rate increment must be positive");
            }

            if (model.RateIncrement > (model.UpperBoundRate - model.LowerBoundRate))
            {
                result.Errors.Add("Rate increment cannot be larger than the rate range");
            }
        }
    }
}
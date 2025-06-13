using NPVCalculator.Client.Interfaces;
using NPVCalculator.Client.Models;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Services
{
    public class NpvCalculationService : INpvCalculationService
    {
        private readonly INpvService _npvService;
        private readonly IInputValidationService _inputValidator;

        public NpvCalculationService(INpvService npvService, IInputValidationService inputValidator)
        {
            _npvService = npvService ?? throw new ArgumentNullException(nameof(npvService));
            _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
        }

        public async Task<NpvCalculationResult> ProcessCalculationAsync(NpvInputModel inputModel)
        {
            try
            {
                var validationResult = _inputValidator.ValidateInput(inputModel);
                if (!validationResult.IsValid)
                {
                    return NpvCalculationResult.ValidationFailure(validationResult.Errors);
                }

                var request = inputModel.ToNpvRequest();
                var response = await _npvService.CalculateNpvAsync(request);

                return response.IsSuccess
                    ? NpvCalculationResult.Success(response.Data)
                    : NpvCalculationResult.ServiceFailure(response.Errors);
            }
            catch (Exception ex)
            {
                return NpvCalculationResult.ExceptionFailure(ex.Message);
            }
        }
    }

    public class NpvCalculationResult
    {
        public bool IsSuccess { get; private set; }
        public List<NpvResult>? Results { get; private set; }
        public List<string> Errors { get; private set; } = [];
        public NpvCalculationResultType ResultType { get; private set; }

        private NpvCalculationResult() { }

        public static NpvCalculationResult Success(List<NpvResult>? results)
        {
            return new NpvCalculationResult
            {
                IsSuccess = true,
                Results = results,
                ResultType = NpvCalculationResultType.Success
            };
        }

        public static NpvCalculationResult ValidationFailure(IEnumerable<string> errors)
        {
            return new NpvCalculationResult
            {
                IsSuccess = false,
                Errors = errors.ToList(),
                ResultType = NpvCalculationResultType.ValidationFailure
            };
        }

        public static NpvCalculationResult ServiceFailure(List<string>? errors)
        {
            return new NpvCalculationResult
            {
                IsSuccess = false,
                Errors = errors ?? [],
                ResultType = NpvCalculationResultType.ServiceFailure
            };
        }

        public static NpvCalculationResult ExceptionFailure(string errorMessage)
        {
            return new NpvCalculationResult
            {
                IsSuccess = false,
                Errors = [$"Error: {errorMessage}"],
                ResultType = NpvCalculationResultType.ExceptionFailure
            };
        }
    }

    public enum NpvCalculationResultType
    {
        Success,
        ValidationFailure,
        ServiceFailure,
        ExceptionFailure
    }
}
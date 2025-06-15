using NPVCalculator.Client.Interfaces;
using NPVCalculator.Client.Models;
using NPVCalculator.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace NPVCalculator.Client.Services
{
    public class NpvService : INpvService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NpvService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public NpvService(HttpClient httpClient, ILogger<NpvService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<ApiResponse<List<NpvResult>>> CalculateNpvAsync(NpvRequest request)
        {
            try
            {
                _logger.LogInformation("Calling NPV API with {CashFlowCount} cash flows",
                    request?.CashFlows?.Count ?? 0);

                var httpResponse = await _httpClient.PostAsJsonAsync("api/npv/calculate", request, _jsonOptions);

                return httpResponse.IsSuccessStatusCode
                    ? await HandleSuccessResponse(httpResponse)
                    : await HandleErrorResponse(httpResponse);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling NPV API");
                return CreateErrorResponse("Network error. Please check your connection.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex,"Request timeout or cancellation");
                return CreateErrorResponse("Request timed out. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling NPV API");
                return CreateErrorResponse("An unexpected error occurred.");
            }
        }

        private async Task<ApiResponse<List<NpvResult>>> HandleSuccessResponse(HttpResponseMessage httpResponse)
        {
            try
            {
                var response = await httpResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                if (response.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
                {
                    var data = new List<NpvResult>();
                    var warnings = new List<string>();

                    if (response.TryGetProperty("data", out var dataProp))
                    {
                        data = JsonSerializer.Deserialize<List<NpvResult>>(dataProp.GetRawText(), _jsonOptions) ?? [];
                    }

                    if (response.TryGetProperty("warnings", out var warningsProp))
                    {
                        warnings = JsonSerializer.Deserialize<List<string>>(warningsProp.GetRawText(), _jsonOptions) ?? [];
                    }

                    return new ApiResponse<List<NpvResult>>
                    {
                        IsSuccess = true,
                        Data = data,
                        Errors = warnings.Select(w => $"Warning: {w}").ToList()
                    };
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse API response");
            }

            return CreateErrorResponse("Unable to parse API response");
        }

        private async Task<ApiResponse<List<NpvResult>>> HandleErrorResponse(HttpResponseMessage httpResponse)
        {
            var errorContent = await httpResponse.Content.ReadAsStringAsync();

            try
            {
                var errorResponse = JsonSerializer.Deserialize<JsonElement>(errorContent, _jsonOptions);

                if (errorResponse.TryGetProperty("success", out var successProp) &&
                    !successProp.GetBoolean() &&
                    errorResponse.TryGetProperty("errors", out var errorsProp))
                {
                    var errors = JsonSerializer.Deserialize<string[]>(errorsProp.GetRawText(), _jsonOptions);
                    if (errors?.Any() == true)
                    {
                        return new ApiResponse<List<NpvResult>>
                        {
                            IsSuccess = false,
                            Errors = errors.ToList(),
                            ErrorMessage = "Validation errors occurred"
                        };
                    }
                }
            }
            catch (JsonException)
            {
                // Fall through to generic error
            }

            var errorMessage = httpResponse.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "Invalid request data",
                System.Net.HttpStatusCode.InternalServerError => "Server error occurred",
                _ => $"API error: {httpResponse.StatusCode}"
            };

            return CreateErrorResponse(errorMessage);
        }

        private static ApiResponse<List<NpvResult>> CreateErrorResponse(string errorMessage) =>
            new()
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Errors = [errorMessage]
            };
    }
}
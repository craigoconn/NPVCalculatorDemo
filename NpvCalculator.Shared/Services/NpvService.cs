using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NpvCalculator.Shared.Services;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Services
{
    public class NpvService : INpvService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NpvService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public NpvService(HttpClient httpClient, ILogger<NpvService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<ApiResponse<List<NpvResult>>> CalculateNpvAsync(NpvRequest request)
        {
            var response = new ApiResponse<List<NpvResult>>
            {
                IsSuccess = false,
                Errors = new List<string>()
            };

            try
            {
                _logger.LogInformation("Starting NPV calculation with {CashFlowCount} cash flows",
                    request.CashFlows?.Count ?? 0);

                if (request == null)
                {
                    response.ErrorMessage = "Request cannot be null";
                    response.Errors.Add("Invalid request data");
                    return response;
                }

                if (request.CashFlows == null || !request.CashFlows.Any())
                {
                    response.ErrorMessage = "Cash flows are required";
                    response.Errors.Add("At least one cash flow must be provided");
                    return response;
                }

                var httpResponse = await _httpClient.PostAsJsonAsync("api/npv/calculate", request, _jsonOptions);

                _logger.LogInformation("HTTP response received with status: {StatusCode}", httpResponse.StatusCode);

                if (httpResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        var apiResponse = await httpResponse.Content.ReadFromJsonAsync<ApiResponseWrapper>(_jsonOptions);
                        if (apiResponse?.Success == true && apiResponse.Data != null)
                        {
                            response.IsSuccess = true;
                            response.Data = apiResponse.Data;
                            _logger.LogInformation("Successfully calculated NPV for {ResultCount} rates", apiResponse.Data.Count);
                            return response;
                        }
                    }
                    catch (JsonException)
                    {
                        // Fall back to old format
                    }
                }
                else
                {
                    await HandleErrorResponse(httpResponse, response);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while calculating NPV");
                response.ErrorMessage = "Network error. Please check your connection and try again.";
                response.Errors.Add($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "Request timeout occurred while calculating NPV");
                response.ErrorMessage = "Request timed out. Please try again.";
                response.Errors.Add("Request timeout");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request was cancelled while calculating NPV");
                response.ErrorMessage = "Request was cancelled. Please try again.";
                response.Errors.Add("Request cancelled");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error occurred while calculating NPV");
                response.ErrorMessage = "Error processing server response. Please try again.";
                response.Errors.Add($"Data format error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while calculating NPV");
                response.ErrorMessage = "An unexpected error occurred. Please try again.";
                response.Errors.Add($"Unexpected error: {ex.Message}");
            }

            return response;
        }

        private async Task HandleErrorResponse(HttpResponseMessage httpResponse, ApiResponse<List<NpvResult>> response)
        {
            var errorContent = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogWarning("API returned error status {StatusCode}: {ErrorContent}",
                httpResponse.StatusCode, errorContent);

            // Try to parse as structured error response first
            if (!string.IsNullOrWhiteSpace(errorContent))
            {
                try
                {
                    var problemDetails = JsonSerializer.Deserialize<ValidationProblemDetails>(errorContent, _jsonOptions);
                    if (problemDetails?.Errors != null)
                    {
                        response.Errors.AddRange(problemDetails.Errors.SelectMany(e => e.Value));
                        response.ErrorMessage = problemDetails.Title ?? "Validation errors occurred";
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Not a problem details response, try custom error format
                }

                // Try parsing as simple string array
                try
                {
                    var errorArray = JsonSerializer.Deserialize<string[]>(errorContent, _jsonOptions);
                    if (errorArray != null && errorArray.Any())
                    {
                        response.Errors.AddRange(errorArray);
                        response.ErrorMessage = "Server validation errors occurred";
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Not a string array
                }
            }

            // Fallback to generic error message based on status code
            response.ErrorMessage = httpResponse.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "Invalid request data",
                System.Net.HttpStatusCode.Unauthorized => "Authentication required",
                System.Net.HttpStatusCode.Forbidden => "Access denied",
                System.Net.HttpStatusCode.NotFound => "Service endpoint not found",
                System.Net.HttpStatusCode.InternalServerError => "Internal server error",
                System.Net.HttpStatusCode.ServiceUnavailable => "Service temporarily unavailable",
                _ => $"Server error: {httpResponse.StatusCode}"
            };

            response.Errors.Add($"HTTP {(int)httpResponse.StatusCode}: {httpResponse.ReasonPhrase}");

            // Include raw error content
            if (!string.IsNullOrWhiteSpace(errorContent) && errorContent.Length < 500)
            {
                response.Errors.Add($"Server response: {errorContent}");
            }
        }

        // ASP.NET Core validation problem details
        private class ValidationProblemDetails
        {
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }

        private class ApiResponseWrapper
        {
            public bool Success { get; set; }
            public List<NpvResult>? Data { get; set; }
            public List<string>? Warnings { get; set; }
        }

    }
}
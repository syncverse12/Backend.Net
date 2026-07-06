using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI;
using SyncVerse.Application.Interfaces.AI;

namespace SyncVerse.Infrastructure.Services.AI
{
    public class AttritionPredictionService : IAttritionPredictionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public AttritionPredictionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("AI_Attrition_Server");
            _apiKey = configuration["AIApiSettings:AttritionApiKey"] ?? ""; // Ensure this is set in appsettings.json
        }

        public async Task<Result<AttritionPredictionResponseDto>> PredictAttritionAsync(string employeeId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/attrition/predict/{employeeId}");

                // Add Authentication Header
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle specific error codes if needed (e.g., 404, 422, 503)
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<AttritionPredictionResponseDto>.Failure($"Failed to predict attrition: {response.StatusCode} - {errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Maps python snake_case to C# PascalCase
                };

                var prediction = JsonSerializer.Deserialize<AttritionPredictionResponseDto>(content, options);

                if (prediction == null)
                {
                     return Result<AttritionPredictionResponseDto>.Failure("Failed to deserialize the AI response.");
                }

                return Result<AttritionPredictionResponseDto>.Success(prediction);
            }
            catch (Exception ex)
            {
                return Result<AttritionPredictionResponseDto>.Failure($"An error occurred while calling the AI service: {ex.Message}");
            }
        }
    }
}
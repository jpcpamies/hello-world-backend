using HelloWorldBackend.Configuration;
using HelloWorldBackend.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace HelloWorldBackend.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAISettings _openAISettings;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(HttpClient httpClient, IOptions<OpenAISettings> openAISettings, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _openAISettings = openAISettings.Value;
        _logger = logger;
        
        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_openAISettings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAISettings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "HelloWorldBackend/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_openAISettings.TimeoutSeconds);
    }

    public async Task<TextSummaryResponse> SummarizeTextAsync(TextSummaryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting text summarization for {TextLength} characters", request.Text.Length);

            var prompt = CreateSummarizationPrompt(request);
            var requestBody = new
            {
                model = _openAISettings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that creates concise summaries and bullet points from text." },
                    new { role = "user", content = prompt }
                },
                max_tokens = request.MaxSummaryLength * 2, // Allow for bullet points
                temperature = 0.3
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/chat/completions", httpContent, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API request failed with status {StatusCode}: {ErrorContent}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API request failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var choice = openAIResponse?.Choices?.FirstOrDefault();
            var messageContent = choice?.Message?.Content;
            
            if (string.IsNullOrEmpty(messageContent))
            {
                throw new InvalidOperationException("Invalid response from OpenAI API");
            }

            var result = ParseSummaryResponse(messageContent);
            result.TokensUsed = openAIResponse?.Usage?.TotalTokens;
            
            _logger.LogInformation("Text summarization completed successfully, tokens used: {TokensUsed}", result.TokensUsed);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during text summarization");
            throw;
        }
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                model = _openAISettings.Model,
                messages = new[]
                {
                    new { role = "user", content = "Hello" }
                },
                max_tokens = 5
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/chat/completions", httpContent, cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI health check failed");
            return false;
        }
    }

    private string CreateSummarizationPrompt(TextSummaryRequest request)
    {
        return $@"Please summarize the following text and provide bullet points of the key information.

Format your response exactly as follows:
SUMMARY: [Your summary here, max {request.MaxSummaryLength} characters]

BULLET POINTS:
• [First key point]
• [Second key point]
• [Third key point]
[Continue with more bullet points as needed]

Text to summarize:
{request.Text}";
    }

    private TextSummaryResponse ParseSummaryResponse(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var summary = string.Empty;
        var bulletPoints = new List<string>();
        var inBulletPointsSection = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            if (trimmedLine.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
            {
                summary = trimmedLine.Substring(8).Trim();
            }
            else if (trimmedLine.Equals("BULLET POINTS:", StringComparison.OrdinalIgnoreCase))
            {
                inBulletPointsSection = true;
            }
            else if (inBulletPointsSection && (trimmedLine.StartsWith("•") || trimmedLine.StartsWith("-") || trimmedLine.StartsWith("*")))
            {
                bulletPoints.Add(trimmedLine.Substring(1).Trim());
            }
        }

        // Fallback if parsing fails
        if (string.IsNullOrEmpty(summary))
        {
            summary = content.Length > 500 ? content.Substring(0, 500) + "..." : content;
        }

        return new TextSummaryResponse
        {
            Summary = summary,
            BulletPoints = bulletPoints,
            ProcessedAt = DateTime.UtcNow
        };
    }

    private class OpenAIResponse
    {
        public Choice[]? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int TotalTokens { get; set; }
    }
}
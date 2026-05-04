using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AIChatBot.Services
{
    public class AIService : IAIService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public AIService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        public async Task<string> GetAIResponseAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            var apiKey = _config["AI:ApiKey"];
            // Default to Mistral chat completions endpoint and model when not provided
            var endpoint = _config["AI:Endpoint"] ?? "https://api.mistral.ai/v1/chat/completions";
            var model = _config["AI:Model"] ?? "mistral-small-latest";

            // If apiKey not configured, return a mock response
            if (string.IsNullOrEmpty(apiKey))
            {
                return $"This is AI response for: {message}";
            }

            try
            {
                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "AIChatBot/1.0");

                var requestBody = new
                {
                    model = model,
                    messages = new[] { new { role = "user", content = message } }
                };

                var json = JsonSerializer.Serialize(requestBody);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"AI API error: {response.StatusCode} - {responseString}";
                }

                using var doc = JsonDocument.Parse(responseString);
                // Try common response shapes (OpenAI-like, Mistral-like)
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // OpenAI / Mistral chat-completions: { choices: [ { message: { content: "..." } } ] }
                    if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var first = choices[0];
                        if (first.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.Object)
                        {
                            if (messageProp.TryGetProperty("content", out var contentProp))
                                return contentProp.GetString() ?? string.Empty;
                            if (messageProp.TryGetProperty("text", out var textProp))
                                return textProp.GetString() ?? string.Empty;
                        }

                        if (first.TryGetProperty("text", out var textProp2))
                        {
                            return textProp2.GetString() ?? string.Empty;
                        }
                    }

                    // Some APIs return { result: { output: [...] } } or results array
                    if (doc.RootElement.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Object)
                    {
                        if (result.TryGetProperty("output", out var output) && output.ValueKind == JsonValueKind.Array && output.GetArrayLength() > 0)
                        {
                            var firstOut = output[0];
                            if (firstOut.ValueKind == JsonValueKind.String)
                                return firstOut.GetString() ?? string.Empty;
                            if (firstOut.ValueKind == JsonValueKind.Object && firstOut.TryGetProperty("content", out var c))
                                return c.GetString() ?? string.Empty;
                        }
                    }

                    // Some models return results array with output_text
                    if (doc.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                    {
                        var r0 = results[0];
                        if (r0.TryGetProperty("output_text", out var outText))
                            return outText.GetString() ?? string.Empty;
                    }
                }

                // Fallback: return raw response string
                return responseString;
            }
            catch (Exception ex)
            {
                return $"AI Exception: {ex.Message}";
            }
        }
    }
}

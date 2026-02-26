using System.Text;
using System.Text.Json;
using PhishingDetector.Core.Interfaces;

namespace PhishingDetector.API.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<OllamaService> _logger;
    public OllamaService(HttpClient httpClient, IConfiguration config, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<string> AnalyzeAsync(string prompt, CancellationToken ct = default)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(8));

        try
        {
            var model = _config["Ollama:Model"] ?? "glm-5:cloud";
            var requestBody = new
            {
                model,
                prompt,
                stream = false,
                options = new { temperature = 0.1, num_predict = 2048 }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/generate", content, cts.Token);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            using var doc = JsonDocument.Parse(responseJson);

            string rawResponse = doc.RootElement.GetProperty("response").GetString() ?? string.Empty;

            return rawResponse.Replace("```json", "").Replace("```", "").Trim();
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Ollama 8 dakika içinde yanıt vermedi");
            throw new TimeoutException("Ollama zaman aşımı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama API call failed");
            throw;
        }
    }
    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/tags", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

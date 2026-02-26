using PhishingDetector.Core.DTOs;
using PhishingDetector.Core.Models;

namespace PhishingDetector.Web.Services;

public class PhishingApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PhishingApiClient> _logger;

    public PhishingApiClient(HttpClient http, ILogger<PhishingApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AnalyzeEmailResponse?> AnalyzeAsync(AnalyzeEmailRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/phishing/analyze", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new Exception(error);
            }
            return await response.Content.ReadFromJsonAsync<AnalyzeEmailResponse>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "API analyze call failed");
            throw;
        }
    }

    public async Task<DashboardStats?> GetDashboardAsync(CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<DashboardStats>("api/phishing/dashboard", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dashboard call failed");
            return null;
        }
    }

    public async Task<List<EmailAnalysis>?> GetHistoryAsync(int count = 20, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<EmailAnalysis>>($"api/phishing/history?count={count}", ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "History call failed");
            return null;
        }
    }

    public async Task<(bool ok, bool ollamaOk)> HealthCheckAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<HealthResponse>("api/phishing/health", ct);
            return (true, result?.Ollama == "connected");
        }
        catch
        {
            return (false, false);
        }
    }

    private record HealthResponse(string Status, string Ollama);
}

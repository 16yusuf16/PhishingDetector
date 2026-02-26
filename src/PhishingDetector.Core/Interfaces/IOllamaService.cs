namespace PhishingDetector.Core.Interfaces;

public interface IOllamaService
{
    Task<string> AnalyzeAsync(string prompt, CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
}

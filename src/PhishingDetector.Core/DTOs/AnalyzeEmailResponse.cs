namespace PhishingDetector.Core.DTOs;

public class AnalyzeEmailResponse
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string OverallRisk { get; set; } = string.Empty;
    public int OverallRiskScore { get; set; }
    public int UrlRiskScore { get; set; }
    public int HeaderRiskScore { get; set; }
    public int ContentRiskScore { get; set; }
    public string UrlAnalysis { get; set; } = string.Empty;
    public string HeaderAnalysis { get; set; } = string.Empty;
    public string ContentAnalysis { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Indicators { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
}

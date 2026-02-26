namespace PhishingDetector.Core.Models;

public enum RiskLevel
{
    Normal = 0,
    Spam = 1,
    Dangerous = 2
}

public class EmailAnalysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Headers { get; set; } = string.Empty;
    public string Urls { get; set; } = string.Empty;

    public RiskLevel OverallRisk { get; set; }
    public int UrlRiskScore { get; set; }
    public int HeaderRiskScore { get; set; }
    public int ContentRiskScore { get; set; }
    public int OverallRiskScore { get; set; }

    public string UrlAnalysis { get; set; } = string.Empty;
    public string HeaderAnalysis { get; set; } = string.Empty;
    public string ContentAnalysis { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public List<string> Indicators { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public long ProcessingTimeMs { get; set; }
}

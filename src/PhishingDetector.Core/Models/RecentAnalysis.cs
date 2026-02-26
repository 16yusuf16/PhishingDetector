namespace PhishingDetector.Core.Models;

public class RecentAnalysis
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public RiskLevel Risk { get; set; }
    public int Score { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

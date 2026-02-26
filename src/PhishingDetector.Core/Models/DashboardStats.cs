namespace PhishingDetector.Core.Models;

public class DashboardStats
{
    public int TotalAnalyzed { get; set; }
    public int NormalCount { get; set; }
    public int SpamCount { get; set; }
    public int DangerousCount { get; set; }
    public double AverageRiskScore { get; set; }
    public List<DailyStatPoint> Last7Days { get; set; } = new();
    public List<RecentAnalysis> RecentAnalyses { get; set; } = new();
}

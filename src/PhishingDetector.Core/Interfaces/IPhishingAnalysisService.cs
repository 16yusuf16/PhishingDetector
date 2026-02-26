using PhishingDetector.Core.DTOs;
using PhishingDetector.Core.Models;

namespace PhishingDetector.Core.Interfaces;

public interface IPhishingAnalysisService
{
    Task<AnalyzeEmailResponse> AnalyzeEmailAsync(AnalyzeEmailRequest request, CancellationToken ct = default);
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);
    Task<List<EmailAnalysis>> GetRecentAnalysesAsync(int count = 20, CancellationToken ct = default);
    Task<EmailAnalysis?> GetAnalysisByIdAsync(Guid id, CancellationToken ct = default);
}

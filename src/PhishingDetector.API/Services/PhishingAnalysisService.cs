using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PhishingDetector.API.Data;
using PhishingDetector.Core.DTOs;
using PhishingDetector.Core.Interfaces;
using PhishingDetector.Core.Models;

namespace PhishingDetector.API.Services;

public class PhishingAnalysisService : IPhishingAnalysisService
{
    private readonly IOllamaService _ollama;
    private readonly PhishingDbContext _db;
    private readonly ILogger<PhishingAnalysisService> _logger;

    public PhishingAnalysisService(IOllamaService ollama, PhishingDbContext db, ILogger<PhishingAnalysisService> logger)
    {
        _ollama = ollama;
        _db = db;
        _logger = logger;
    }

    public async Task<AnalyzeEmailResponse> AnalyzeEmailAsync(AnalyzeEmailRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        //var (urlScore, urlAnalysis) = await AnalyzeUrlsAsync(request.Urls, ct);
        //var (headerScore, headerAnalysis) = await AnalyzeHeadersAsync(request.Headers, request.Sender, ct);
        //var (contentScore, contentAnalysis) = await AnalyzeContentAsync(request.Subject, request.Content, ct);
        var urlsTask = AnalyzeUrlsAsync(request.Urls, ct);
        var headersTask = AnalyzeHeadersAsync(request.Headers, request.Sender, ct);
        var contentTask = AnalyzeContentAsync(request.Subject, request.Content, ct);

      
        await Task.WhenAll(urlsTask, headersTask, contentTask);

        var (urlScore, urlAnalysis) = await urlsTask;
        var (headerScore, headerAnalysis) = await headersTask;
        var (contentScore, contentAnalysis) = await contentTask;


        int overallScore = (int)Math.Round(urlScore * 0.4 + headerScore * 0.3 + contentScore * 0.3);

        var riskLevel = overallScore switch
        {
            >= 70 => RiskLevel.Dangerous,
            >= 35 => RiskLevel.Spam,
            _ => RiskLevel.Normal
        };

        var summaryPrompt = $"""
                Görev:
                Verilen analiz sonuçlarına dayanarak 2 veya en fazla 3 cümlelik Türkçe bir özet üret.

                Kurallar:
                - SADECE özet metnini yaz.
                - Ek açıklama yazma.
                - Yeni bilgi ekleme.
                - Varsayım yapma.
                - Girdi içinde olmayan hiçbir şeyi üretme.
                - Risk skorunu ve risk seviyesini aynen kullan.
                - URL, Header ve İçerik analizlerinde yazanları özetle, yorum katma.

                Girdi:
                Risk Skoru: {overallScore}/100 ({riskLevel})
                URL Analizi: {urlAnalysis}
                Header Analizi: {headerAnalysis}
                İçerik Analizi: {contentAnalysis}
                """;

        var summary = await _ollama.AnalyzeAsync(summaryPrompt, ct);

        var indicators = ExtractIndicators(request, urlScore, headerScore, contentScore);

        sw.Stop();

        var analysis = new EmailAnalysis
        {
            Subject = request.Subject,
            Sender = request.Sender,
            Content = request.Content,
            Headers = request.Headers,
            Urls = request.Urls,
            OverallRisk = riskLevel,
            UrlRiskScore = urlScore,
            HeaderRiskScore = headerScore,
            ContentRiskScore = contentScore,
            OverallRiskScore = overallScore,
            UrlAnalysis = urlAnalysis,
            HeaderAnalysis = headerAnalysis,
            ContentAnalysis = contentAnalysis,
            Summary = summary.Trim(),
            Indicators = indicators,
            ProcessingTimeMs = sw.ElapsedMilliseconds
        };

        _db.EmailAnalyses.Add(analysis);
        await _db.SaveChangesAsync(CancellationToken.None); 

        return MapToResponse(analysis);
    }

    private async Task<(int score, string analysis)> AnalyzeUrlsAsync(string urls, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(urls))
            return (0, "URL bulunamadı.");

        var prompt = $$"""
                Bir phishing URL analiz sistemisin.

                KURALLAR:
                - SADECE geçerli JSON çıktısı üret.
                - JSON dışında hiçbir metin yazma.
                - Açıklama, yorum, düşünce süreci veya ek not yazma.
                - Yalnızca verilen URL'lere dayanarak analiz yap.
                - URL içinde açıkça görülmeyen hiçbir bilgi hakkında varsayım yapma.
                - Domain itibarı, şirket geçmişi veya dış veri kaynağı kullanıyormuş gibi davranma.
                - Eksik bilgi varsa analysis alanında belirt.
                - Skoru yalnızca URL yapısında bulunan somut risk sinyallerine göre belirle.

                ÇIKTI FORMATI (birebir uy):
                {"score": <0-100>, "analysis": "<Türkçe>"}

                DEĞERLENDİRME KRİTERLERİ:
                - IP adresi ile erişim
                - URL kısaltıcı servis kullanımı
                - HTTPS eksikliği
                - Domain içinde harf benzerliği / typo-squatting (örn: paypa1, rnicrosoft vb.)
                - Aşırı uzun ve karmaşık query parametreleri
                - Şüpheli alt domain yapısı (örn: login.secure.account.verify.example.com gibi)


                GİRDİ:
                URL'ler: {{urls}}
                """;

        try
        {
            var result = await _ollama.AnalyzeAsync(prompt, ct);
            return ParseScoreResponse(result, "URL analizi tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "URL analysis failed");
            return (50, "URL analizi sırasında hata oluştu.");
        }
    }

    private async Task<(int score, string analysis)> AnalyzeHeadersAsync(string headers, string sender, CancellationToken ct)
    {
        var headerValue = string.IsNullOrWhiteSpace(headers) ? "none" : headers;
        var prompt = $$"""
                Bir phishing e-posta header analiz sistemisin.

                KURALLAR:
                - SADECE geçerli JSON çıktısı üret.
                - JSON dışında hiçbir metin yazma.
                - Açıklama, yorum, düşünce süreci, ek not yazma.
                - Sadece verilen Sender ve Headers içeriğine dayanarak analiz yap.
                - Varsayım yapma.
                - Eksik bilgi varsa bunu analiz alanında açıkça belirt.
                - Skor üretirken yalnızca somut bulgulara dayan.

                ÇIKTI FORMATI (birebir uy):
                {"score": <0-100>, "analysis": "<Türkçe>"}

                DEĞERLENDİRME KRİTERLERİ:
                - SPF sonucu (pass/fail/softfail/none)
                - DKIM sonucu
                - DMARC sonucu
                - Sender domain ile header domain uyumu
                - Return-Path uyumu
                - Şüpheli IP adresi kullanımı
                - Forged veya tutarsız header izleri

                GİRDİ:
                Sender: {{sender}}
                Headers: {{headerValue}}
                """;

        try
        {
            var result = await _ollama.AnalyzeAsync(prompt, ct);
            return ParseScoreResponse(result, "Header analizi tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Header analysis failed");
            return (50, "Header analizi sırasında hata oluştu.");
        }
    }

    private async Task<(int score, string analysis)> AnalyzeContentAsync(string subject, string content, CancellationToken ct)
    {
        var prompt = $$"""
                Bir phishing e-posta içerik analiz sistemisin.

                KURALLAR:
                - SADECE geçerli JSON çıktısı üret.
                - JSON dışında hiçbir metin yazma.
                - Açıklama, yorum, düşünce süreci, ek not yazma.
                - Yalnızca verilen Subject ve Content metnine dayanarak analiz yap.
                - Metinde açıkça geçmeyen hiçbir marka, kurum veya olay hakkında varsayım yapma.
                - Eksik bilgi varsa analysis alanında belirt.
                - Skoru yalnızca metinde bulunan somut risk sinyallerine göre belirle.

                ÇIKTI FORMATI (birebir uy):
                {"score": <0-100>, "analysis": "<Türkçe>"}

                DEĞERLENDİRME KRİTERLERİ:
                - Aciliyet veya tehdit dili (örn: "hemen", "son uyarı", "hesabınız kapatılacak")
                - Kimlik bilgisi / şifre / kredi kartı talebi
                - Marka veya kurum taklidi (metinde açıkça geçiyorsa)
                - Ödül, çekiliş, miras, para vaadi
                - Şüpheli yönlendirme veya doğrulama isteği


                GİRDİ:
                Subject: {{subject}}
                Content: {{content}}
                """;

        try
        {
            var result = await _ollama.AnalyzeAsync(prompt, ct);
            return ParseScoreResponse(result, "İçerik analizi tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Content analysis failed");
            return (50, "İçerik analizi sırasında hata oluştu.");
        }
    }

    private static (int score, string analysis) ParseScoreResponse(string raw, string fallback)
    {
        try
        {
            var cleaned = Regex.Replace(raw, @"```json|```", "").Trim();
            var start = cleaned.IndexOf('{');
            var end = cleaned.LastIndexOf('}');
            if (start >= 0 && end > start)
                cleaned = cleaned[start..(end + 1)];

            using var doc = JsonDocument.Parse(cleaned);
            var score = doc.RootElement.GetProperty("score").GetInt32();
            var analysis = doc.RootElement.GetProperty("analysis").GetString() ?? fallback;
            return (Math.Clamp(score, 0, 100), analysis);
        }
        catch
        {
            var numMatch = Regex.Match(raw, @"\b(\d{1,3})\b");
            if (numMatch.Success && int.TryParse(numMatch.Value, out var score))
                return (Math.Clamp(score, 0, 100), raw.Length > 200 ? raw[..200] : raw);
            return (50, fallback);
        }
    }

    private static List<string> ExtractIndicators(AnalyzeEmailRequest req, int urlScore, int headerScore, int contentScore)
    {
        var indicators = new List<string>();

        if (urlScore >= 70) indicators.Add("🔴 Tehlikeli URL'ler tespit edildi");
        else if (urlScore >= 35) indicators.Add("🟡 Şüpheli URL'ler tespit edildi");

        if (headerScore >= 70) indicators.Add("🔴 E-posta header'ları sahte görünüyor");
        else if (headerScore >= 35) indicators.Add("🟡 Header anomalileri tespit edildi");

        if (contentScore >= 70) indicators.Add("🔴 İçerik yüksek phishing riski taşıyor");
        else if (contentScore >= 35) indicators.Add("🟡 Şüpheli içerik örüntüleri tespit edildi");

        var combined = $"{req.Subject} {req.Content}".ToLowerInvariant();
        if (Regex.IsMatch(combined, @"\b(urgent|acil|hemen|şimdi|immediately)\b"))
            indicators.Add("⚠️ Aciliyet dili kullanılmış");
        if (Regex.IsMatch(combined, @"\b(şifre|password|credential|hesap|account|verify|doğrula)\b"))
            indicators.Add("⚠️ Kimlik bilgisi isteği içeriyor");
        if (Regex.IsMatch(combined, @"\b(kazandınız|winner|prize|ödül|lottery|çekiliş)\b"))
            indicators.Add("⚠️ Ödül/piyango iddiası içeriyor");
        if (Regex.IsMatch(combined, @"\b(banka|bank|paypal|amazon|microsoft|google|apple)\b", RegexOptions.IgnoreCase))
            indicators.Add("⚠️ Güvenilir marka taklidi içerebilir");

        if (!string.IsNullOrWhiteSpace(req.Urls))
        {
            var urls = req.Urls.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (urls.Any(u => Regex.IsMatch(u, @"\b(bit\.ly|tinyurl|t\.co|goo\.gl|ow\.ly)\b")))
                indicators.Add("⚠️ Kısaltılmış URL'ler tespit edildi");
            if (urls.Any(u => Regex.IsMatch(u, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")))
                indicators.Add("🔴 IP adresi tabanlı URL'ler tespit edildi");
        }

        if (indicators.Count == 0)
            indicators.Add("✅ Belirgin phishing göstergesi tespit edilmedi");

        return indicators;
    }

    private static AnalyzeEmailResponse MapToResponse(EmailAnalysis a) => new()
    {
        Id = a.Id,
        Subject = a.Subject,
        Sender = a.Sender,
        OverallRisk = a.OverallRisk.ToString(),
        OverallRiskScore = a.OverallRiskScore,
        UrlRiskScore = a.UrlRiskScore,
        HeaderRiskScore = a.HeaderRiskScore,
        ContentRiskScore = a.ContentRiskScore,
        UrlAnalysis = a.UrlAnalysis,
        HeaderAnalysis = a.HeaderAnalysis,
        ContentAnalysis = a.ContentAnalysis,
        Summary = a.Summary,
        Indicators = a.Indicators,
        AnalyzedAt = a.AnalyzedAt,
        ProcessingTimeMs = a.ProcessingTimeMs
    };

    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var stats = await _db.EmailAnalyses
            .GroupBy(e => e.OverallRisk)
            .Select(g => new { Risk = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var normal = stats.FirstOrDefault(s => s.Risk == RiskLevel.Normal)?.Count ?? 0;
        var spam = stats.FirstOrDefault(s => s.Risk == RiskLevel.Spam)?.Count ?? 0;
        var dangerous = stats.FirstOrDefault(s => s.Risk == RiskLevel.Dangerous)?.Count ?? 0;
        var total = stats.Sum(s => s.Count);

        var avgScore = total > 0
            ? await _db.EmailAnalyses.AverageAsync(e => (double)e.OverallRiskScore, ct)
            : 0;

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var last7Days = await _db.EmailAnalyses
            .Where(e => e.AnalyzedAt >= sevenDaysAgo)
            .ToListAsync(ct);

        var dailyStats = last7Days
            .GroupBy(e => e.AnalyzedAt.Date)
            .Select(g => new DailyStatPoint
            {
                Date = g.Key,
                Normal = g.Count(e => e.OverallRisk == RiskLevel.Normal),
                Spam = g.Count(e => e.OverallRisk == RiskLevel.Spam),
                Dangerous = g.Count(e => e.OverallRisk == RiskLevel.Dangerous)
            })
            .OrderBy(d => d.Date)
            .ToList();

        var recent = await _db.EmailAnalyses
            .OrderByDescending(e => e.AnalyzedAt)
            .Take(10)
            .Select(e => new RecentAnalysis
            {
                Id = e.Id,
                Subject = e.Subject,
                Sender = e.Sender,
                Risk = e.OverallRisk,
                Score = e.OverallRiskScore,
                AnalyzedAt = e.AnalyzedAt
            })
            .ToListAsync(ct);

        return new DashboardStats
        {
            TotalAnalyzed = total,
            NormalCount = normal,
            SpamCount = spam,
            DangerousCount = dangerous,
            AverageRiskScore = Math.Round(avgScore, 1),
            Last7Days = dailyStats,
            RecentAnalyses = recent
        };
    }

    public async Task<List<EmailAnalysis>> GetRecentAnalysesAsync(int count = 20, CancellationToken ct = default)
        => await _db.EmailAnalyses.OrderByDescending(e => e.AnalyzedAt).Take(count).ToListAsync(ct);

    public async Task<EmailAnalysis?> GetAnalysisByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.EmailAnalyses.FindAsync(new object[] { id }, ct);
}
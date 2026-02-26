namespace PhishingDetector.Core.DTOs;

public class AnalyzeEmailRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Headers { get; set; } = string.Empty;
    public string Urls { get; set; } = string.Empty;
}

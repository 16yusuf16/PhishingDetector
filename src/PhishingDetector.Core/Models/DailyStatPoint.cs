namespace PhishingDetector.Core.Models;

public class DailyStatPoint
{
    public DateTime Date { get; set; }
    public int Normal { get; set; }
    public int Spam { get; set; }
    public int Dangerous { get; set; }
}

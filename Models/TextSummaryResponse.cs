namespace HelloWorldBackend.Models;

public class TextSummaryResponse
{
    public string Summary { get; set; } = string.Empty;
    public List<string> BulletPoints { get; set; } = new();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public int? TokensUsed { get; set; }
}
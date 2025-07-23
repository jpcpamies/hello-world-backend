namespace HelloWorldBackend.Models;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public Dictionary<string, object> Services { get; set; } = new();
}
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HelloWorldBackend.Services;

public class OpenAIHealthCheck : IHealthCheck
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<OpenAIHealthCheck> _logger;

    public OpenAIHealthCheck(IOpenAIService openAIService, ILogger<OpenAIHealthCheck> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _openAIService.IsHealthyAsync(cancellationToken);
            
            if (isHealthy)
            {
                return HealthCheckResult.Healthy("OpenAI service is responding");
            }
            else
            {
                return HealthCheckResult.Degraded("OpenAI service is not responding");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI health check failed");
            return HealthCheckResult.Unhealthy("OpenAI service health check failed", ex);
        }
    }
}
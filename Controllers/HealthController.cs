using Microsoft.AspNetCore.Mvc;
using HelloWorldBackend.Models;
using HelloWorldBackend.Services;
using System.Reflection;

namespace HelloWorldBackend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IOpenAIService openAIService, ILogger<HealthController> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the health status of the API and its dependencies
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health status information</returns>
    [HttpGet]
    [ProducesResponseType<HealthCheckResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<HealthCheckResponse>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthCheckResponse>> GetHealth(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Performing health check");

            var services = new Dictionary<string, object>();
            var overallStatus = "Healthy";

            // Check OpenAI service
            var openAIHealthy = await _openAIService.IsHealthyAsync(cancellationToken);
            services["OpenAI"] = new
            {
                Status = openAIHealthy ? "Healthy" : "Unhealthy",
                LastChecked = DateTime.UtcNow
            };

            if (!openAIHealthy)
            {
                overallStatus = "Degraded";
            }

            // Check database (if we had one)
            services["Database"] = new
            {
                Status = "N/A",
                LastChecked = DateTime.UtcNow,
                Note = "No database configured"
            };

            var response = new HealthCheckResponse
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Version = GetVersion(),
                Services = services
            };

            var statusCode = overallStatus == "Healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
            
            _logger.LogInformation("Health check completed with status: {Status}", overallStatus);
            
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during health check");
            
            var errorResponse = new HealthCheckResponse
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Version = GetVersion(),
                Services = new Dictionary<string, object>
                {
                    ["Error"] = new
                    {
                        Status = "Error",
                        Message = "Health check failed",
                        LastChecked = DateTime.UtcNow
                    }
                }
            };

            return StatusCode(StatusCodes.Status503ServiceUnavailable, errorResponse);
        }
    }

    private string GetVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
}
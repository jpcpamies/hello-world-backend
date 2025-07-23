using Microsoft.AspNetCore.Mvc;
using HelloWorldBackend.Models;
using HelloWorldBackend.Services;
using FluentValidation;

namespace HelloWorldBackend.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TextController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly IValidator<TextSummaryRequest> _validator;
    private readonly ILogger<TextController> _logger;

    public TextController(IOpenAIService openAIService, IValidator<TextSummaryRequest> validator, ILogger<TextController> logger)
    {
        _openAIService = openAIService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Summarizes the provided text using AI and returns key bullet points
    /// </summary>
    /// <param name="request">The text summarization request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A summary and bullet points of the provided text</returns>
    [HttpPost("summarize")]
    [ProducesResponseType<TextSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TextSummaryResponse>> SummarizeText(
        [FromBody] TextSummaryRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var problemDetails = new ValidationProblemDetails();
                foreach (var error in validationResult.Errors)
                {
                    if (!problemDetails.Errors.ContainsKey(error.PropertyName))
                    {
                        problemDetails.Errors[error.PropertyName] = new string[] { };
                    }
                    problemDetails.Errors[error.PropertyName] = problemDetails.Errors[error.PropertyName]
                        .Concat(new[] { error.ErrorMessage }).ToArray();
                }
                
                return BadRequest(problemDetails);
            }

            _logger.LogInformation("Processing text summarization request for {TextLength} characters", 
                request.Text.Length);

            // Process the summarization
            var response = await _openAIService.SummarizeTextAsync(request, cancellationToken);
            
            _logger.LogInformation("Text summarization completed successfully");
            
            return Ok(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "External service error during text summarization");
            return Problem(
                title: "External Service Error",
                detail: "An error occurred while processing your request with the AI service.",
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Timeout occurred during text summarization");
            return Problem(
                title: "Request Timeout",
                detail: "The request took too long to process. Please try again.",
                statusCode: StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during text summarization");
            return Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred while processing your request.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
using HelloWorldBackend.Models;

namespace HelloWorldBackend.Services;

public interface IOpenAIService
{
    Task<TextSummaryResponse> SummarizeTextAsync(TextSummaryRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
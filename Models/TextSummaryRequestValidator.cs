using FluentValidation;

namespace HelloWorldBackend.Models;

public class TextSummaryRequestValidator : AbstractValidator<TextSummaryRequest>
{
    public TextSummaryRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required")
            .Length(10, 10000)
            .WithMessage("Text must be between 10 and 10000 characters");

        RuleFor(x => x.MaxSummaryLength)
            .GreaterThanOrEqualTo(50)
            .WithMessage("MaxSummaryLength must be at least 50")
            .LessThanOrEqualTo(1000)
            .WithMessage("MaxSummaryLength must not exceed 1000");
    }
}
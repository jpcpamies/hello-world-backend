using System.ComponentModel.DataAnnotations;

namespace HelloWorldBackend.Models;

public class TextSummaryRequest
{
    [Required(ErrorMessage = "Text is required")]
    [StringLength(10000, MinimumLength = 10, ErrorMessage = "Text must be between 10 and 10000 characters")]
    public string Text { get; set; } = string.Empty;

    [Range(50, 1000, ErrorMessage = "MaxSummaryLength must be between 50 and 1000")]
    public int MaxSummaryLength { get; set; } = 200;
}
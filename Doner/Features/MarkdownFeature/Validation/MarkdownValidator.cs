using FluentValidation;

namespace Doner.Features.MarkdownFeature.Validation;

public class MarkdownValidator : AbstractValidator<Markdown>
{
    public MarkdownValidator()
    {
        RuleFor(m => m.Title)
            .NotEmpty()
            .WithMessage("Title cannot be empty");
    }
}

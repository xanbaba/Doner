using FluentValidation;

namespace Doner.Features.MarkdownFeature;

public class MarkdownValidator : AbstractValidator<Markdown>
{
    public MarkdownValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Markdown name must not be empty.")
            .MaximumLength(100);

        RuleFor(x => x.Uri)
            .NotEmpty()
            .WithMessage("Markdown URI must not be empty.")
            .MaximumLength(200);

        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .WithMessage("Workspace ID must not be empty.");
    }
}
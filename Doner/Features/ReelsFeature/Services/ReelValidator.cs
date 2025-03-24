using FluentValidation;

namespace Doner.Features.ReelsFeature.Services;

public class ReelValidator : AbstractValidator<Reel>
{
    public ReelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
        
        RuleFor(x => x.WorkspaceId)
            .NotEmpty();
        
        RuleFor(x => x.OwnerId)
            .NotEmpty();
    }
}
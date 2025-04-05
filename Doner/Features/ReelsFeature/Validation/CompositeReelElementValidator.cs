using Doner.Features.ReelsFeature.Elements;
using FluentValidation;

namespace Doner.Features.ReelsFeature.Validation;

public class CompositeReelElementValidator : AbstractValidator<ReelElement>
{
    public CompositeReelElementValidator
    (
        IValidator<Picture> pictureValidator,
        IValidator<Checkbox> checkboxValidator,
        IValidator<Dropdown> dropdownValidator,
        IValidator<PlainText> plainTextValidator
    )
    {
        RuleFor(x => x)
            .SetInheritanceValidator(v =>
            {
                v.Add(pictureValidator);
                v.Add(checkboxValidator);
                v.Add(dropdownValidator);
                v.Add(plainTextValidator);
            });
    }
}
using Doner.Features.ReelsFeature.Elements;
using FluentValidation;

namespace Doner.Features.ReelsFeature.Validation;

public class PictureValidator : AbstractValidator<Picture>
{
    public PictureValidator()
    {
        RuleFor(p => p.Url).NotEmpty().WithMessage("URL is required.");
        RuleFor(p => p.Width).GreaterThan(0).WithMessage("Width must be greater than 0.");
        RuleFor(p => p.Height).GreaterThan(0).WithMessage("Height must be greater than 0.");
        RuleFor(p => p.Caption).MaximumLength(500).WithMessage("Caption cannot exceed 500 characters.");
    }
}

public class CheckboxValidator : AbstractValidator<Checkbox>
{
    public CheckboxValidator()
    {
        RuleFor(c => c.Header).NotEmpty().WithMessage("Header is required.");
    }
}

public class DropdownValidator : AbstractValidator<Dropdown>
{
    public DropdownValidator()
    {
        RuleFor(d => d.Elements).NotEmpty().WithMessage("Dropdown must contain at least one element.");
    }
}

public class PlainTextValidator : AbstractValidator<PlainText>
{
    public PlainTextValidator()
    {
        RuleFor(p => p.Text).NotEmpty().WithMessage("Text is required.");
    }
}
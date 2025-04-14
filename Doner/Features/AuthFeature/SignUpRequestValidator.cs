using Contracts.V1.Requests;
using FluentValidation;

namespace Doner.Features.AuthFeature;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(u => u.Email).EmailAddress().NotEmpty().MaximumLength(255)
            .When(u => u.Email is not null);
        RuleFor(u => u.Username).NotEmpty().Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Username must contain only letters, numbers, and underscores.");
        RuleFor(u => u.Login).NotEmpty().Length(8, 100)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Login must contain only letters, numbers, and underscores.");
        RuleFor(u => u.Password).NotEmpty().Length(8, 255)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&_]{8,}$")
            .WithMessage(
                "Password must be at least 8 characters long, contain at least one uppercase and one lowercase letter, one number and one special character(@, $, !, %, *, ?, &, _). Any other symbols are not allowed.");
    }
}
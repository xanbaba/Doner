using Contracts.V1.Requests;
using FluentValidation;

namespace Doner.Features.AuthFeature;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Login)
            .Matches("^[a-zA-Z0-9_]+$")
            .WithMessage("Login must contain only letters, numbers, and underscores.");
        RuleFor(x => x.Password).NotEmpty();
    }
}
using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Le token est requis.");
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Le refresh token est requis.");
    }
}

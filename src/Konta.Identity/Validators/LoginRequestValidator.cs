using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("Le format de l'email est invalide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis.");
    }
}

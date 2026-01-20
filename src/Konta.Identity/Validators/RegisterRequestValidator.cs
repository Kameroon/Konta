using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("Le nom du tenant est requis.")
            .MaximumLength(100).WithMessage("Le nom du tenant ne doit pas dépasser 100 caractères.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("Le format de l'email est invalide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis.")
            .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères.")
            .Matches("[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule.")
            .Matches("[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule.")
            .Matches("[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est requis.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est requis.");
    }
}

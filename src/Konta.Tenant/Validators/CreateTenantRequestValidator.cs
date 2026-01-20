using FluentValidation;
using Konta.Tenant.DTOs;

namespace Konta.Tenant.Validators;

/// <summary>
/// Validateur pour la création d'un tenant.
/// </summary>
public class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    /// <summary>
    /// Initialise les règles de validation pour la création d'un tenant.
    /// </summary>
    public CreateTenantRequestValidator()
    {
        // Le nom de l'entreprise est obligatoire
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom de l'entreprise est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        // L'identifiant (slug/domaine) est obligatoire
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("L'identifiant unique est obligatoire.")
            .Matches("^[a-z0-9-]+$").WithMessage("L'identifiant ne doit contenir que des lettres minuscules, chiffres et tirets.")
            .MaximumLength(50).WithMessage("L'identifiant ne peut pas dépasser 50 caractères.");

        // Le secteur d'activité
        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Le secteur d'activité ne peut pas dépasser 100 caractères.");
    }
}

using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class UpdateTenantRequestValidator : AbstractValidator<UpdateTenantRequest>
{
    public UpdateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du tenant est obligatoire.")
            .MaximumLength(100).WithMessage("Le nom ne peut pas dépasser 100 caractères.");

        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Le plan est obligatoire.")
            .MaximumLength(50).WithMessage("Le plan ne peut pas dépasser 50 caractères.");
    }
}

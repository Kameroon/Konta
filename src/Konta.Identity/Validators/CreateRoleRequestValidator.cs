using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du rôle est requis.")
            .MaximumLength(50).WithMessage("Le nom du rôle ne doit pas dépasser 50 caractères.");
        
        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("La description ne doit pas dépasser 200 caractères.");
    }
}

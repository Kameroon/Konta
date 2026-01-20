using FluentValidation;
using Konta.Identity.DTOs;

namespace Konta.Identity.Validators;

public class AssignPermissionRequestValidator : AbstractValidator<AssignPermissionRequest>
{
    public AssignPermissionRequestValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("L'ID de la permission est requis.");
    }
}

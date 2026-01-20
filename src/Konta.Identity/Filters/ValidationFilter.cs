using FluentValidation;
using Konta.Shared.Responses;

namespace Konta.Identity.Filters;

public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is not null)
        {
            var argument = context.Arguments.OfType<T>().FirstOrDefault();
            if (argument is not null)
            {
                var validationResult = await validator.ValidateAsync(argument);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(ApiResponse.Fail("Erreur de validation", errors));
                }
            }
        }

        return await next(context);
    }
}

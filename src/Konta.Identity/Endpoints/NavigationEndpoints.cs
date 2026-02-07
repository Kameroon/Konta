using Konta.Identity.DTOs;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Konta.Identity.Endpoints;

public static class NavigationEndpoints
{
    public static void MapNavigationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/navigation")
            .WithTags("Navigation")
            .RequireAuthorization();

        group.MapGet("", async (INavigationService navigationService) =>
        {
            var items = await navigationService.GetAllAsync();
            return Results.Ok(items);
        })
        .WithName("GetNavigationItems");

        group.MapPut("/{id:guid}", async (Guid id, UpdateNavigationItemRequest request, INavigationService navigationService) =>
        {
            var success = await navigationService.UpdateAsync(id, request);
            return success 
                ? Results.Ok(ApiResponse<object>.Ok(null, "Élément de menu mis à jour."))
                : Results.NotFound(ApiResponse<object>.Fail("Élément introuvable."));
        })
        .WithName("UpdateNavigationItem")
        .RequireAuthorization(policy => policy.RequireRole("SuperAdmin"));
    }
}

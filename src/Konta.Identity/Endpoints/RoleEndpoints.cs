using Konta.Identity.DTOs;
using Konta.Identity.Filters;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Konta.Identity.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles")
                       .WithTags("Gestion des Rôles")
                       .RequireAuthorization();

        group.MapPost("/", async (CreateRoleRequest request, IRoleService roleService, HttpContext httpContext) =>
        {
            var tenantIdClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim)) return Results.Unauthorized();

            // Délégation de toute la logique au Service
            var roleId = await roleService.CreateRoleAsync(Guid.Parse(tenantIdClaim), request);
            
            return Results.Ok(ApiResponse<object>.Ok(new { RoleId = roleId }, "Le rôle a été créé avec succès."));
        })
        .WithName("CreateRole")
        .AddEndpointFilter<ValidationFilter<CreateRoleRequest>>()
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/permissions", async (Guid id, AssignPermissionRequest request, IRoleService roleService) =>
        {
            await roleService.AssignPermissionAsync(id, request);
            return Results.Ok(ApiResponse.Ok("La permission a été assignée avec succès."));
        })
        .WithName("AssignPermission")
        .AddEndpointFilter<ValidationFilter<AssignPermissionRequest>>()
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK);
    }
}

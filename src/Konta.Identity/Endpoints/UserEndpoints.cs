using Konta.Identity.DTOs;
using Konta.Identity.Models;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Konta.Identity.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", async (ITenantContext tenantContext, IUserService userService) =>
        {
            if (!tenantContext.TenantId.HasValue) 
                return Results.Unauthorized();

            IEnumerable<User> users;
            if (tenantContext.IsGlobalAdmin)
            {
                users = await userService.GetAllUsersAsync();
            }
            else
            {
                users = await userService.GetAllUsersByTenantIdAsync(tenantContext.TenantId.Value);
            }
            
            return Results.Ok(ApiResponse<IEnumerable<User>>.Ok(users));
        })
        .WithName("GetAllUsers")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateUserRequest request, ITenantContext tenantContext, IUserService userService, IPasswordHasher passwordHasher) =>
        {
            if (!tenantContext.TenantId.HasValue) return Results.Unauthorized();

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = request.Role,
                IsActive = request.IsActive,
                TenantId = request.TenantId ?? tenantContext.TenantId.Value,
                PasswordHash = passwordHasher.Hash(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            var id = await userService.CreateUserAsync(user);
            return Results.Created($"/api/users/{id}", ApiResponse<object>.Ok(new { Id = id }));
        }).WithName("CreateUser");

        group.MapPut("/{id}", async (Guid id, User user, IUserService userService) =>
        {
            user.Id = id;
            var success = await userService.UpdateUserAsync(user);
            return success ? Results.Ok(ApiResponse<object>.Ok(null)) : Results.NotFound(ApiResponse<object>.Fail("Utilisateur introuvable."));
        }).WithName("UpdateUser");

        group.MapDelete("/{id}", async (Guid id, IUserService userService) =>
        {
            var success = await userService.DeleteUserAsync(id);
            return success ? Results.Ok(ApiResponse<object>.Ok(null)) : Results.NotFound(ApiResponse<object>.Fail("Utilisateur introuvable."));
        }).WithName("DeleteUser");
    }
}

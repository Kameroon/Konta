using Konta.Identity.DTOs;
using Konta.Identity.Filters;
using Konta.Identity.Services.Interfaces;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Konta.Identity.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentification");

        group.MapPost("/register", async (RegisterRequest request, ITenantService tenantService) =>
        {
            var userId = await tenantService.RegisterTenantAsync(request);
            return Results.Ok(ApiResponse<object>.Ok(new { UserId = userId }, "Tenant et utilisateur créés avec succès"));
        })
        .WithName("RegisterTenant")
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(response, "Connexion réussie"));
        })
        .WithName("Login")
        .AddEndpointFilter<ValidationFilter<LoginRequest>>()
        .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request.Token, request.RefreshToken);
            return Results.Ok(ApiResponse<TokenResponse>.Ok(response, "Token rafraîchi avec succès"));
        })
        .WithName("RefreshToken")
        .AddEndpointFilter<ValidationFilter<RefreshTokenRequest>>()
        .Produces<ApiResponse<TokenResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse<object>>(StatusCodes.Status401Unauthorized);
    }
}

using Konta.Billing.DTOs;
using Konta.Billing.Services.Interfaces;
using Konta.Billing.Services.Implementations;
using Konta.Billing.Data.Repositories.Interfaces;
using Konta.Billing.Models;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Konta.Billing.Endpoints;

public static class BillingEndpoints
{
    /// <summary>
    /// Enregistre les points de terminaison HTTP pour le microservice de facturation.
    /// </summary>
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing").WithTags("Facturation & Paiement");

        /// <summary>
        /// GET /api/billing/plans
        /// Récupère la liste de tous les plans d'abonnement actifs (Public).
        /// </summary>
        group.MapGet("/plans", async (ISubscriptionPlanRepository planRepository) =>
        {
            var plans = await planRepository.GetAllActiveAsync();
            return Results.Ok(ApiResponse<IEnumerable<SubscriptionPlan>>.Ok(plans, "Plans récupérés."));
        })
        .AllowAnonymous()
        .WithName("GetPlans");

        /// <summary>
        /// POST /api/billing/checkout
        /// Initialise une session de paiement Stripe pour un plan donné.
        /// </summary>
        group.MapPost("/checkout", async (CheckoutRequest request, IStripeService stripeService, HttpContext context) =>
        {
            // Note: Le tenantId devrait normalement être extrait du JWT claim
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder pour démo
            
            var url = await stripeService.CreateCheckoutSessionAsync(tenantId, request.PlanCode, request.PriceId);
            return Results.Ok(ApiResponse<CheckoutResponse>.Ok(new CheckoutResponse(url), "Session créée."));
        })
        .RequireAuthorization()
        .WithName("CreateCheckout");

        /// <summary>
        /// GET /api/billing/portal
        /// Génère une URL pour le portail de gestion d'abonnement Stripe (Self-service).
        /// </summary>
        group.MapGet("/portal", async (IStripeService stripeService) =>
        {
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder
            var url = await stripeService.CreateCustomerPortalSessionAsync(tenantId);
            return Results.Ok(ApiResponse<string>.Ok(url, "URL du portail récupérée."));
        })
        .RequireAuthorization();

        /// <summary>
        /// POST /webhooks/stripe
        /// Point de terminaison public pour recevoir les notifications asynchrones de Stripe.
        /// </summary>
        app.MapPost("/webhooks/stripe", async (HttpRequest request, WebhookHandler handler) =>
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();
            var signature = request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(signature)) return Results.BadRequest();

            await handler.HandleAsync(json, signature!);
            return Results.Ok();
        })
        .AllowAnonymous()
        .WithName("StripeWebhook");
    }
}

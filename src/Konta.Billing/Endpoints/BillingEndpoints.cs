using Konta.Billing.DTOs;
using Konta.Billing.Services.Interfaces;
using Konta.Billing.Services.Implementations;
using Konta.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Konta.Billing.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/billing").WithTags("Facturation & Paiement");

        // POST : Créer une session de paiement
        group.MapPost("/checkout", async (CheckoutRequest request, IStripeService stripeService, HttpContext context) =>
        {
            // Note: Le tenantId devrait normalement être extrait du JWT claim
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder pour démo
            
            var url = await stripeService.CreateCheckoutSessionAsync(tenantId, request.PlanCode, request.PriceId);
            return Results.Ok(ApiResponse<CheckoutResponse>.Ok(new CheckoutResponse(url), "Session créée."));
        })
        .RequireAuthorization()
        .WithName("CreateCheckout");

        // GET : Accéder au portail client Stripe
        group.MapGet("/portal", async (IStripeService stripeService) =>
        {
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder
            var url = await stripeService.CreateCustomerPortalSessionAsync(tenantId);
            return Results.Ok(ApiResponse<string>.Ok(url, "URL du portail récupérée."));
        })
        .RequireAuthorization();

        // POST : Webhook Stripe (Endpoint public mais signé)
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

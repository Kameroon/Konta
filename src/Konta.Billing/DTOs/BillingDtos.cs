namespace Konta.Billing.DTOs;

public record CheckoutRequest(string PlanCode, string PriceId);
public record CheckoutResponse(string SessionUrl);
public record SubscriptionStatusResponse(string StripeCustomerId, string Status, string? PlanCode);

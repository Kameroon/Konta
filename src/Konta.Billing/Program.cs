using Konta.Billing.Extensions;
using Konta.Billing.Endpoints;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Enregistrement des Services
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("stripecustomers_tenantid_key", "Ce client possède déjà un identifiant Stripe.");
        options.UniqueViolations.Add("webhookevents_stripeeventid_key", "Cet événement a déjà été traité.");
    })
    .AddBillingInfrastructure(builder.Configuration)
    .AddBillingServices()
    .AddAuthenticationConfig(builder.Configuration)
    .AddObservability("Konta.Billing", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// 2. Pipeline HTTP
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Konta Billing API v1"));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantContext();

// Redirection automatique vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// 3. Mapping des Endpoints
app.MapBillingEndpoints();

app.Run();

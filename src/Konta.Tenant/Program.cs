using Konta.Tenant.Extensions;
using Konta.Tenant.Endpoints;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Services
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("subscriptionplans_code_key", "Ce code de plan d'abonnement existe déjà.");
        options.UniqueViolations.Add("tenants_identifier_key", "Cet identifiant d'entreprise est déjà utilisé.");
        options.UniqueViolations.Add("subscriptions_tenantid_key", "Cette entreprise possède déjà un abonnement actif.");
    })
    .AddInfrastructure(builder.Configuration)
    .AddApplicationServices()
    .AddAuthenticationConfig(builder.Configuration)
    .AddObservability("Konta.Tenant", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// 2. Configure Pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseTenantContext();

// 3. Map Endpoints
app.MapTenantEndpoints();

app.Run();

using Konta.Identity.Endpoints;
using Konta.Identity.Extensions;
using Konta.Tenant.Extensions;
using Konta.Tenant.Endpoints;
using Konta.Billing.Extensions;
using Konta.Billing.Endpoints;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Identity");

// 1. Register Services
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("users_email_key", "Cette adresse email est déjà utilisée.");
        options.UniqueViolations.Add("permissions_systemname_key", "Ce nom de système de permission existe déjà.");
        options.UniqueViolations.Add("refreshtokens_token_key", "Ce jeton de rafraîchissement est déjà utilisé.");
        options.UniqueViolations.Add("tenants_name_key", "Ce nom d'entreprise est déjà utilisée.");
    })
    .AddSharedInfrastructure(builder.Configuration)
    .AddAuthenticationConfig(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddExceptionHandler<Konta.Shared.Middleware.GlobalExceptionHandler>()
    .AddProblemDetails()
    .AddIdentityInfrastructure(builder.Configuration)
    .AddIdentityServices()
    .AddTenantInfrastructure(builder.Configuration)
    .AddTenantServices()
    .AddBillingInfrastructure(builder.Configuration)
    .AddBillingServices();

var app = builder.Build();

// Database Initialization
using (var scope = app.Services.CreateScope())
{
    // Initialize Identity
    var identityInitializer = scope.ServiceProvider.GetRequiredService<Konta.Identity.Data.DatabaseInitializer>();
    await identityInitializer.InitializeAsync();

    // Initialize Tenant
    var tenantInitializer = scope.ServiceProvider.GetRequiredService<Konta.Tenant.Data.DatabaseInitializer>();
    await tenantInitializer.InitializeAsync();

    // Initialize Billing
    var billingInitializer = scope.ServiceProvider.GetRequiredService<Konta.Billing.Data.DatabaseInitializer>();
    await billingInitializer.InitializeAsync();
}

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

// Redirection automatique vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// 3. Map Endpoints
app.MapAuthEndpoints();
app.MapRoleEndpoints();
app.MapTenantEndpoints(); // Now maps the unified ITenantService
app.MapUserEndpoints();
app.MapNavigationEndpoints();
app.MapBillingEndpoints();

app.Run();

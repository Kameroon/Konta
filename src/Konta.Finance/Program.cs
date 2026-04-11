using Konta.Finance.Endpoints;
using Konta.Finance.Extensions;
using Konta.Finance.Core.Endpoints;
using Konta.Finance.Core.Extensions;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Finance");

// Add services to the container.
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("accounts_tenantid_code_key", "Ce code de compte existe déjà pour votre entreprise.");
        options.UniqueViolations.Add("journals_tenantid_code_key", "Ce code de journal existe déjà.");
        options.UniqueViolations.Add("budgets_category_key", "Un budget pour cette catégorie existe déjà sur cette période.");
        options.UniqueViolations.Add("tiers_taxid_key", "Ce numéro d'identifiant fiscal est déjà enregistré pour un autre tiers.");
    })
    .AddFinanceInfrastructure(builder.Configuration)
    .AddFinanceServices()
    .AddFinanceCoreInfrastructure(builder.Configuration)
    .AddFinanceCoreServices()
    .AddObservability("Konta.API.Finance", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    // Initialize Finance (Accounting)
    var financeInitializer = scope.ServiceProvider.GetRequiredService<Konta.Finance.Data.DatabaseInitializer>();
    await financeInitializer.InitializeAsync();

    // Initialize Finance Core (Budgets, Treasury)
    var financeCoreInitializer = scope.ServiceProvider.GetRequiredService<Konta.Finance.Core.Data.DatabaseInitializer>();
    await financeCoreInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
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

// Map Endpoints
app.MapFinanceEndpoints();
app.MapFinanceCoreEndpoints();

app.Run();

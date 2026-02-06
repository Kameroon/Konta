using Konta.Finance.Core.Endpoints;
using Konta.Finance.Core.Extensions;
using Konta.Shared.Extensions;

// --- Démarrage du moteur Konta Finance Core ---

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Finance.Core");

// Configuration des services du conteneur
builder.Services
    .AddSharedServices(options =>
    {
        // Enregistre les traductions d'erreurs Postgres spécifiques au métier financier
        options.UniqueViolations.Add("budgets_category_key", "Un budget pour cette catégorie existe déjà sur cette période.");
        options.UniqueViolations.Add("tiers_taxid_key", "Ce numéro d'identifiant fiscal est déjà enregistré pour un autre tiers.");
    })
    .AddFinanceCoreInfrastructure(builder.Configuration) // Infras : DB & Auth
    .AddFinanceCoreServices() // Services métier : Budget, Trésorerie, Invoices
    .AddObservability("Konta.Finance.Core", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Konta.Finance.Core.Data.DatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configuration du pipeline de traitement des requêtes (Middleware)
if (app.Environment.IsDevelopment())
{
    // Activation de Swagger pour l'exploration de l'API en dev
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Konta Finance Core API v1");
    });
}

// Sécurité : redirection HTTPS et Authentification
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantContext();

// Redirection automatique vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Enregistrement des routes API
app.MapFinanceCoreEndpoints();

// Lancement de l'écoute des requêtes
app.Run();

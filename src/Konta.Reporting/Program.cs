using Konta.Reporting.Endpoints;
using Konta.Reporting.Extensions;
using Konta.Shared.Extensions;

// --- Démarrage du moteur analytique Konta Reporting ---

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Reporting");

// Configuration des briques logicielles
builder.Services
    .AddSharedServices(options =>
    {
        // Pas de violations de contraintes spécifiques pour le reporting pour l'instant
    })
    .AddReportingInfrastructure(builder.Configuration)
    .AddReportingServices()
    .AddObservability("Konta.Reporting", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Konta.Reporting.Data.DatabaseInitializer>();
    await initializer.InitializeAsync();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Konta Reporting API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseTenantContext();

// Redirection automatique vers Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Enregistrement des routes analytiques
app.MapReportingEndpoints();

// Lancement du service
app.Run();

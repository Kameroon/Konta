using Konta.Reporting.Endpoints;
using Konta.Reporting.Extensions;
using Konta.Shared.Extensions;

// --- Démarrage du moteur analytique Konta Reporting ---

var builder = WebApplication.CreateBuilder(args);

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

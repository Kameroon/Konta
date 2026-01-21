using Konta.Finance.Endpoints;
using Konta.Finance.Extensions;
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
    })
    .AddFinanceInfrastructure(builder.Configuration)
    .AddFinanceServices()
    .AddObservability("Konta.Finance", builder.Configuration)
    .AddResilience();

var app = builder.Build();

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

app.Run();

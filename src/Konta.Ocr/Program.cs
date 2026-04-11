using Konta.Ocr.Endpoints;
using Konta.Ocr.Extensions;
using Konta.Shared.Extensions;

// Enregistrer les handlers Dapper pour DateOnly/TimeOnly
DapperExtensions.AddDateOnlyHandlers();

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Ocr");

// Add services to the container.
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("webhookevents_stripeeventid_key", "Cet événement a déjà été traité.");
    })
    .AddSharedInfrastructure(builder.Configuration)
    .AddAuthenticationConfig(builder.Configuration)
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddExceptionHandler<Konta.Shared.Middleware.GlobalExceptionHandler>()
    .AddProblemDetails()
    .AddOcrInfrastructure(builder.Configuration)
    .AddOcrServices()
    .AddObservability("Konta.Ocr", builder.Configuration)
    .AddResilience();

var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Konta.Ocr.Data.DatabaseInitializer>();
    await initializer.InitializeAsync();
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
app.MapOcrEndpoints();

app.Run();

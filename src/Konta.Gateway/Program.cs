using Ocelot.Middleware;
using Konta.Gateway.Extensions;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog pour les logs centralisés
builder.AddSerilogLogging("Konta.Gateway");

// 1. Configuration
builder.Configuration.AddOcelotConfig(builder.Environment);

// 2. Services
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// 3. Pipeline HTTP
app.UseDefaultFiles(); // Active index.html comme page par défaut
app.UseStaticFiles(); // Support des fichiers statiques (index.html)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// 4. Ocelot Middleware
await app.UseOcelot();

app.Run();

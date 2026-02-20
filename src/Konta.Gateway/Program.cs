using Ocelot.Middleware;
using Konta.Gateway.Extensions;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging("Konta.Gateway");

// Configuration Ocelot
builder.Configuration.AddOcelotConfig(builder.Environment);

builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => {
    endpoints.MapGet("/healthz", () => $"READY-OK-{DateTime.UtcNow:yyyyMMddHHmm}");
});

await app.UseOcelot();

app.Run();

using Konta.Ocr.Endpoints;
using Konta.Ocr.Extensions;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("webhookevents_stripeeventid_key", "Cet événement a déjà été traité.");
    })
    .AddOcrInfrastructure(builder.Configuration)
    .AddOcrServices()
    .AddObservability("Konta.Ocr", builder.Configuration)
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

// Map Endpoints
app.MapOcrEndpoints();

app.Run();

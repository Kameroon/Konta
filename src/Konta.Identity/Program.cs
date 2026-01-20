using Konta.Identity.Endpoints;
using Konta.Identity.Extensions;
using Konta.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Services
builder.Services
    .AddSharedServices(options =>
    {
        options.UniqueViolations.Add("users_email_key", "Cette adresse email est déjà utilisée.");
        options.UniqueViolations.Add("permissions_systemname_key", "Ce nom de système de permission existe déjà.");
        options.UniqueViolations.Add("refreshtokens_token_key", "Ce jeton de rafraîchissement est déjà utilisé.");
        options.UniqueViolations.Add("tenants_name_key", "Ce nom d'entreprise est déjà utilisé.");
    })
    .AddInfrastructure(builder.Configuration)
    .AddApplicationServices()
    .AddAuthenticationConfig(builder.Configuration);

var app = builder.Build();

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
app.MapTenantEndpoints();

app.Run();

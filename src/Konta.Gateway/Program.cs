using Ocelot.Middleware;
using Konta.Gateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration
builder.Configuration.AddOcelotConfig(builder.Environment);

// 2. Services
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// 3. Pipeline HTTP
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

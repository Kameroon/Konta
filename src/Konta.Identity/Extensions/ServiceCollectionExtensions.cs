using FluentValidation;
using Konta.Identity.Services.Implementations;
using Konta.Identity.Services.Interfaces;
using Konta.Identity.Data.Repositories.Implementations;
using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Shared.Data;
using Konta.Shared.Extensions;
using Konta.Shared.Middleware;

namespace Konta.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Shared Infrastructure (Includes DbConnectionFactory)
        services.AddSharedInfrastructure(configuration);
        
        // Repositories
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IAuthService, AuthService>();
        
        // External Services
        services.AddHttpClient<ICompanyRegistryService, CompanyRegistryService>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Swagger & OpenApi
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Exception Handling
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}

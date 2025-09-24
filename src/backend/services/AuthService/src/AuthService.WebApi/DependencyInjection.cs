using AuthService.Infrastructure.Postgres;
using AuthService.Infrastructure.Postgres.Options;
using AuthService.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuthService.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAccountsInfrastructure(configuration);
        services.AddAccountsPresentation();
        return services;
    }

    public static IServiceCollection AddAuthServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, PermissionRequirementHandler>());
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                                 ?? throw new ApplicationException("Missing Jwt configuration");

                options.MapInboundClaims = false; // <-- важно для "Permission"
                options.TokenValidationParameters = TokenValidationParametersFactory.CreateWithLifeTime(jwtOptions);
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
    {
        services.AddWebDependencies();
        return services;
    }

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
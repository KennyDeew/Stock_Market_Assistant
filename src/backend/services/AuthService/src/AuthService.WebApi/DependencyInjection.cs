using AuthService.Infrastructure.Postgres;
using AuthService.Infrastructure.Postgres.Options;
using AuthService.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AuthService.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsModule(this IServiceCollection services)
    {
        services.AddAccountsInfrastructure();
        services.AddAccountsPresentation();
        return services;
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAuthorizationHandler, PermissionRequirementHandler>());
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SECTION_NAME);

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(_ => { });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptionsMonitor<JwtOptions>>((opts, jwt) =>
            {
                opts.SaveToken = true;
                opts.MapInboundClaims = false;
                opts.TokenValidationParameters =
                    TokenValidationParametersFactory.CreateWithLifeTime(jwt.CurrentValue);
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        return services;
    }
}
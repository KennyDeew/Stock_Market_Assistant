using AuthService.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddAccountsPresentation(this IServiceCollection services)
    {
        services.AddScoped<IAccountsContract, AccountsContract>();

        return services;
    }
}
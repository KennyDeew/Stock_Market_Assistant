using AuthService.Application.Commands.Users.CheckEmail;
using AuthService.Application.Commands.Users.Login;
using AuthService.Application.Commands.Users.Logout;
using AuthService.Application.Commands.Users.RefreshTokens;
using AuthService.Application.Commands.Users.Register;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Командные хэндлеры
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokensHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<RegisterHandler>();
        services.AddScoped<CheckEmailHandler>();
        return services;
    }
}
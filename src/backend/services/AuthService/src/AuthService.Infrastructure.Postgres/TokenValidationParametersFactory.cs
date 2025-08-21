using System.Text;
using AuthService.Contracts.Models; // для CustomClaims.Role
using AuthService.Infrastructure.Postgres.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Postgres;

public static class TokenValidationParametersFactory
{
    public static TokenValidationParameters CreateWithLifeTime(JwtOptions jwtOptions) =>
        new()
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            RoleClaimType = CustomClaims.Role
        };

    public static TokenValidationParameters CreateWithoutLifeTime(JwtOptions jwtOptions) =>
        new()
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            RoleClaimType = CustomClaims.Role
        };
}
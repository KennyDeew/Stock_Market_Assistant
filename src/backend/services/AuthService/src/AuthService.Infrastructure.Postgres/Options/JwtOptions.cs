namespace AuthService.Infrastructure.Postgres.Options;

public class JwtOptions
{
    public const string SECTION_NAME = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 10;

    public int RefreshTokenLifetimeDays { get; init; } = 1;
}
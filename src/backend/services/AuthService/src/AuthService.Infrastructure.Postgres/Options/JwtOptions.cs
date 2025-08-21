namespace AuthService.Infrastructure.Postgres.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public int ExpiredMinutes { get; init; } = 15;
}
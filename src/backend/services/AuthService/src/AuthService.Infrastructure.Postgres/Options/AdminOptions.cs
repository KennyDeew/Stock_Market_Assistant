namespace AuthService.Infrastructure.Postgres.Options;

public class AdminOptions
{
    public const string SectionName = "ADMIN";

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
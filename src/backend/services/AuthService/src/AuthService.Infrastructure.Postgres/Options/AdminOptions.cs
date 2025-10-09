namespace AuthService.Infrastructure.Postgres.Options;

public sealed class AdminOptions
{
    public const string SECTION_NAME = "DefaultAdministrator";

    public bool Apply { get; init; } = true;

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}